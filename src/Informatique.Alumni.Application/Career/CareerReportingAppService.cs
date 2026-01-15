using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Branches;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Informatique.Alumni.Career;

[Authorize(AlumniPermissions.Career.Manage)]
public class CareerReportingAppService : ApplicationService, ICareerReportingAppService
{
    private readonly IRepository<AlumniCareerSubscription, Guid> _subscriptionRepository;
    private readonly IRepository<CareerService, Guid> _serviceRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<Education, Guid> _educationRepository;
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IRepository<CareerServiceType, Guid> _serviceTypeRepository;
    private readonly IRepository<College, Guid> _collegeRepository;
    private readonly IRepository<Major, Guid> _majorRepository;
    private readonly Volo.Abp.Identity.IIdentityUserRepository _identityUserRepository;

    public CareerReportingAppService(
        IRepository<AlumniCareerSubscription, Guid> subscriptionRepository,
        IRepository<CareerService, Guid> serviceRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<Education, Guid> educationRepository,
        IRepository<Branch, Guid> branchRepository,
        IRepository<CareerServiceType, Guid> serviceTypeRepository,
        IRepository<College, Guid> collegeRepository,
        IRepository<Major, Guid> majorRepository,
        Volo.Abp.Identity.IIdentityUserRepository identityUserRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _serviceRepository = serviceRepository;
        _profileRepository = profileRepository;
        _educationRepository = educationRepository;
        _branchRepository = branchRepository;
        _serviceTypeRepository = serviceTypeRepository;
        _collegeRepository = collegeRepository;
        _majorRepository = majorRepository;
        _identityUserRepository = identityUserRepository;
    }

    public async Task<List<object>> GetParticipantsReportAsync(CareerParticipantReportInputDto input)
    {
        // 1. Base Query: Subscriptions
        var subscriptions = await _subscriptionRepository.GetQueryableAsync();
        var services = await _serviceRepository.WithDetailsAsync(x => x.Timeslots); // Include Timeslots for Date check
        var profiles = await _profileRepository.GetQueryableAsync();
        var educations = await _educationRepository.GetQueryableAsync();
        var branches = await _branchRepository.GetQueryableAsync();
        var serviceTypes = await _serviceTypeRepository.GetQueryableAsync();
        var colleges = await _collegeRepository.GetQueryableAsync();
        var majors = await _majorRepository.GetQueryableAsync();

        // 2. Join
        var query = from sub in subscriptions
                    join srv in services on sub.CareerServiceId equals srv.Id
                    join prof in profiles on sub.AlumniId equals prof.Id
                    join branch in branches on prof.BranchId equals branch.Id
                    join st in serviceTypes on srv.ServiceTypeId equals st.Id
                    select new { sub, srv, prof, branch, st };

        // Apply Filters (Service)
        query = query.Where(x => x.srv.ServiceTypeId == input.CareerServiceTypeId);
        
        // Date Filter is tricky on Service level since service has multiple dates.
        // Rule: If ANY timeslot falls in range? Or derived "Service Date"? 
        // Logic: Filter if the Service has any timeslot in range.
        // LINQ Translation might fail with complex child check.
        // Better: Check if `sub.TimeslotId` (which we have!) matches a timeslot in range?
        // We didn't join Timeslot table, but we can.
        // Let's optimize: Join Sub -> Timeslot.
        
        // Re-do Join to include Timeslot for accurate filtering
        // We cannot access _timeslotRepository here efficiently if not injected or joined.
        // CareerService contains Timeslots collection. 
        // `x.srv.Timeslots.Any(...)` check.
        
        query = query.Where(x => x.srv.Timeslots.Any(t => t.Date >= input.FromDate && t.Date <= input.ToDate));
        
        // Apply Filters (Branch)
        query = query.Where(x => x.prof.BranchId == input.BranchId);

        // 3. Demographics (Education Filters)
        if (input.CollegeId.HasValue || input.MajorId.HasValue || (input.GraduationYear != null && input.GraduationYear.Any()))
        {
             var matchingAlumniIds = educations.AsQueryable();

             if (input.CollegeId.HasValue)
                 matchingAlumniIds = matchingAlumniIds.Where(e => e.CollegeId == input.CollegeId.Value);

             if (input.MajorId.HasValue)
                 matchingAlumniIds = matchingAlumniIds.Where(e => e.MajorId == input.MajorId.Value);

             if (input.GraduationYear != null && input.GraduationYear.Any())
                 matchingAlumniIds = matchingAlumniIds.Where(e => input.GraduationYear.Contains(e.GraduationYear));

             var allowedIds = matchingAlumniIds.Select(e => e.AlumniProfileId).Distinct();
             
             query = query.Where(x => allowedIds.Contains(x.prof.Id)); 
        }

        // 4. Execution & Projection
        
        if (input.ReportType == CareerReportType.Statistical)
        {
            var stats = query
                .GroupBy(x => new { Name = x.st.NameEn, x.srv.NameEn })
                .Select(g => new CareerParticipantReportStatsDto
                {
                    ServiceTypeName = g.Key.Name,
                    ServiceName = g.Key.NameEn,
                    DateRange = $"{input.FromDate:yyyy-MM-dd} - {input.ToDate:yyyy-MM-dd}",
                    AttendanceCount = g.Count()
                });
                
            return (await AsyncExecuter.ToListAsync(stats)).Cast<object>().ToList();
        }
        else
        {
            // Detailed
            var resultQuery = from q in query
                              let latestEdu = educations.Where(e => e.AlumniProfileId == q.prof.Id).OrderByDescending(e => e.GraduationYear).FirstOrDefault()
                              join col in colleges on latestEdu.CollegeId equals col.Id into colJoin from col in colJoin.DefaultIfEmpty()
                              join maj in majors on latestEdu.MajorId equals maj.Id into majJoin from maj in majJoin.DefaultIfEmpty()
                              select new CareerParticipantReportDetailDto
                              {
                                  SerialNo = 0,
                                  AlumniId = q.prof.UserId, 
                                  AlumniName = "Alumni Name", 
                                  GraduationYear = latestEdu != null ? latestEdu.GraduationYear : 0,
                                  BranchName = q.branch.Name,
                                  CollegeName = col != null ? col.Name : "",
                                  MajorName = maj != null ? maj.Name : "",
                                  ServiceTypeName = q.st.NameEn,
                                  ServiceName = q.srv.NameEn
                              };

            var data = await AsyncExecuter.ToListAsync(resultQuery);
            
            // Post-Processing
            var userIds = data.Select(x => x.AlumniId).Distinct().ToList();
            var users = await _identityUserRepository.GetListByIdsAsync(userIds);
            var userDict = users.ToDictionary(u => u.Id, u => u.Name + " " + u.Surname);

            int index = 1;
            data.ForEach(x => 
            {
                x.SerialNo = index++;
                if (userDict.TryGetValue(x.AlumniId, out var name))
                {
                    x.AlumniName = name;
                }
            });
            
            return data.OrderBy(x => x.AlumniName).Cast<object>().ToList();
        }
    }

    public async Task<List<object>> GetServicesReportAsync(CareerServicesReportInputDto input)
    {
        // 1. Base Query
        var services = await _serviceRepository.WithDetailsAsync(x => x.Timeslots);
        var serviceTypes = await _serviceTypeRepository.GetQueryableAsync();

        var query = from srv in services
                    join st in serviceTypes on srv.ServiceTypeId equals st.Id
                    select new { srv, st };

        // 2. Filters
        query = query.Where(x => x.srv.BranchId == input.BranchId);
        // Filter: Service must have at least one timeslot in range
        query = query.Where(x => x.srv.Timeslots.Any(t => t.Date >= input.FromDate && t.Date <= input.ToDate));

        if (input.CareerServiceTypeId.HasValue)
        {
            query = query.Where(x => x.srv.ServiceTypeId == input.CareerServiceTypeId.Value);
        }

        // 3. Execution & Projection
        if (input.ReportType == CareerReportType.Statistical)
        {
            var stats = query
                .GroupBy(x => x.st.NameEn)
                .Select(g => new CareerServicesReportStatsDto
                {
                    ServiceTypeName = g.Key,
                    Count = g.Count()
                });

            return (await AsyncExecuter.ToListAsync(stats)).Cast<object>().ToList();
        }
        else
        {
            var pagedQuery = query.OrderByDescending(x => x.srv.CreationTime);
            
            // Note: LINQ to SQL with complex child projection (Timeslots) might need iteration.
            // We fetch list then project for safer date range formatting.
            var list = await AsyncExecuter.ToListAsync(pagedQuery);

            var details = list.Select(x => new CareerServicesReportDetailDto
                {
                    SerialNo = 0,
                    ServiceTypeName = x.st.NameEn,
                    ServiceName = x.srv.NameEn,
                    DateRange = FormatDateRange(x.srv),
                    Location = x.srv.Timeslots.FirstOrDefault()?.Room ?? "" // Pick first room? Or "Multiple"?
                }).ToList();

            // Post-Processing: Serial No
            int index = 1;
            details.ForEach(x => x.SerialNo = index++);

            return details.Cast<object>().ToList();
        }
    }

    private string FormatDateRange(CareerService service)
    {
        if (service.Timeslots == null || !service.Timeslots.Any()) return "N/A";
        var min = service.Timeslots.Min(t => t.Date);
        var max = service.Timeslots.Max(t => t.Date);
        if (min.Date == max.Date) return min.ToString("yyyy-MM-dd");
        return $"{min:yyyy-MM-dd} - {max:yyyy-MM-dd}";
    }
}
