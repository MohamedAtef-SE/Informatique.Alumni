using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Branches;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace Informatique.Alumni.Events;

[Authorize(AlumniPermissions.Events.Manage)]
public class ActivityReportingAppService : AlumniAppService
{
    private readonly IRepository<AlumniEventRegistration, Guid> _registrationRepository;
    private readonly IRepository<AssociationEvent, Guid> _eventRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<Education, Guid> _educationRepository;
    private readonly IRepository<ActivityType, Guid> _activityTypeRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IRepository<College, Guid> _collegeRepository;
    private readonly IRepository<Major, Guid> _majorRepository;

    public ActivityReportingAppService(
        IRepository<AlumniEventRegistration, Guid> registrationRepository,
        IRepository<AssociationEvent, Guid> eventRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<Education, Guid> educationRepository,
        IRepository<ActivityType, Guid> activityTypeRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<Branch, Guid> branchRepository,
        IRepository<College, Guid> collegeRepository,
        IRepository<Major, Guid> majorRepository)
    {
        _registrationRepository = registrationRepository;
        _eventRepository = eventRepository;
        _profileRepository = profileRepository;
        _educationRepository = educationRepository;
        _activityTypeRepository = activityTypeRepository;
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _collegeRepository = collegeRepository;
        _majorRepository = majorRepository;
    }

    public async Task<object> GetParticipantsReportAsync(ParticipantReportInputDto input)
    {
        // 1. Get Queryables
        var registrationQuery = await _registrationRepository.GetQueryableAsync();
        var eventQuery = await _eventRepository.WithDetailsAsync(x => x.Timeslots);
        var profileQuery = await _profileRepository.GetQueryableAsync();
        var educationQuery = await _educationRepository.GetQueryableAsync();
        var activityTypeQuery = await _activityTypeRepository.GetQueryableAsync();
        var userQuery = await _userRepository.GetQueryableAsync();
        var branchQuery = await _branchRepository.GetQueryableAsync();
        var collegeQuery = await _collegeRepository.GetQueryableAsync();
        var majorQuery = await _majorRepository.GetQueryableAsync();

        // 2. Build Base Query
        var query = from r in registrationQuery
                    join e in eventQuery on r.EventId equals e.Id
                    join p in profileQuery on r.AlumniId equals p.UserId
                    join u in userQuery on r.AlumniId equals u.Id
                    join b in branchQuery on e.BranchId equals b.Id
                    // Optional Activity Type Join for Name
                    join at in activityTypeQuery on e.ActivityTypeId equals at.Id into atJoin
                    from at in atJoin.DefaultIfEmpty()
                    where r.Status != RegistrationStatus.Cancelled
                    select new { r, e, p, u, b, at };

        // 3. Apply Filters
        query = query.Where(x => x.e.ActivityTypeId == input.ActivityTypeId);
        query = query.Where(x => x.e.BranchId == input.BranchId);

        // Date Range
        query = query.Where(x => x.e.Timeslots.Any(t => t.StartTime >= input.FromDate && t.StartTime <= input.ToDate));

        // Demographics
        if (input.NationalityId.HasValue)
        {
            query = query.Where(x => x.p.NationalityId == input.NationalityId);
        }

        // Education Filters
        var matchedEduQuery = educationQuery.AsQueryable();
        bool eduFilterApplied = false;

        if (input.CollegeId.HasValue)
        {
            matchedEduQuery = matchedEduQuery.Where(edu => edu.CollegeId == input.CollegeId);
            eduFilterApplied = true;
        }
        if (input.MajorId.HasValue)
        {
            matchedEduQuery = matchedEduQuery.Where(edu => edu.MajorId == input.MajorId);
            eduFilterApplied = true;
        }
        if (input.GraduationYear != null && input.GraduationYear.Any())
        {
            matchedEduQuery = matchedEduQuery.Where(edu => input.GraduationYear.Contains(edu.GraduationYear));
            eduFilterApplied = true;
        }

        if (eduFilterApplied)
        {
            query = query.Where(x => matchedEduQuery.Any(edu => edu.AlumniProfileId == x.p.Id));
        }

        // 4. Projection
        if (input.ReportType == ReportType.Detailed)
        {
            // Fetch raw data
            var list = await AsyncExecuter.ToListAsync(query);

            // Fetch needed Educations
            // We need to show the education that matched the filter, or latest if no filter.
            // Since we can't easily join conditionally in one go for projection, we'll fetch educations for these profiles.
            var profileIds = list.Select(x => x.p.Id).Distinct().ToList();
            
            var educations = await AsyncExecuter.ToListAsync(
                educationQuery.Where(e => profileIds.Contains(e.AlumniProfileId))
            );

            // Fetch Colleges and Majors for Names
            var collegeIds = educations.Where(e => e.CollegeId.HasValue).Select(e => e.CollegeId.Value).Distinct().ToList();
            var majorIds = educations.Where(e => e.MajorId.HasValue).Select(e => e.MajorId.Value).Distinct().ToList();

            var colleges = await AsyncExecuter.ToListAsync(collegeQuery.Where(c => collegeIds.Contains(c.Id)));
            var majors = await AsyncExecuter.ToListAsync(majorQuery.Where(m => majorIds.Contains(m.Id)));

            // Prepare Result
            var result = list
                .OrderBy(x => x.u.Name) // Alphabetical by Alumni Name
                .Select((x, index) => 
                {
                    // Find Best Education
                    var studentEdus = educations.Where(e => e.AlumniProfileId == x.p.Id).ToList();
                    Education? bestEdu = null;

                    if (eduFilterApplied)
                    {
                        // Match filter logic
                        bestEdu = studentEdus.FirstOrDefault(e => 
                            (!input.CollegeId.HasValue || e.CollegeId == input.CollegeId) &&
                            (!input.MajorId.HasValue || e.MajorId == input.MajorId) &&
                            ((input.GraduationYear == null || !input.GraduationYear.Any()) || input.GraduationYear.Contains(e.GraduationYear))
                        );
                    }
                    
                    if (bestEdu == null)
                    {
                        bestEdu = studentEdus.OrderByDescending(e => e.GraduationYear).FirstOrDefault();
                    }

                    var collegeName = bestEdu?.CollegeId != null ? colleges.FirstOrDefault(c => c.Id == bestEdu.CollegeId)?.Name : "";
                    var majorName = bestEdu?.MajorId != null ? majors.FirstOrDefault(m => m.Id == bestEdu.MajorId)?.Name : bestEdu?.Degree;

                    return new ParticipantReportDetailDto
                    {
                        SerialNo = index + 1,
                        AlumniId = x.r.AlumniId,
                        AlumniName = x.u.Name + " " + x.u.Surname,
                        GraduationYear = bestEdu?.GraduationYear ?? 0,
                        BranchName = x.b.Name,
                        CollegeName = collegeName ?? "",
                        MajorName = majorName ?? "",
                        ActivityTypeName = x.at?.NameEn ?? "",
                        ActivityName = x.e.NameEn,
                        DateRange = FormatDateRange(x.e.Timeslots)
                    };
                }).ToList();

            return result;
        }
        else
        {
            // Statistical: Group by Event
            // Needs to happen in DB if possible, but date formatting and lookups might be tricky.
            // Grouping by Entity (Event, ActivityType)
            
            var statsQuery = query
                .GroupBy(x => new { x.e.Id, x.e.NameEn, ActivityName = x.at != null ? x.at.NameEn : "" })
                .Select(g => new 
                {
                   EventId = g.Key.Id,
                   EventName = g.Key.NameEn,
                   ActivityTypeName = g.Key.ActivityName,
                   Count = g.Count()
                });

            var stats = await AsyncExecuter.ToListAsync(statsQuery);

            // We need DateRange for each event.
            // Since we grouped by ID, we can fetch timeslots separately or assume we have them if we loaded events?
            // "Summary Table by Event".
            // Logic:
            var eventIds = stats.Select(s => s.EventId).ToList();
            var timeslots = await AsyncExecuter.ToListAsync(
                (await _eventRepository.GetQueryableAsync())
                .Where(e => eventIds.Contains(e.Id))
                .SelectMany(e => e.Timeslots)
            );

            var result = stats.Select(s => new ParticipantReportStatsDto
            {
                ActivityName = s.EventName,
                ActivityTypeName = s.ActivityTypeName,
                AttendanceCount = s.Count,
                DateRange = FormatDateRange(timeslots.Where(t => t.EventId == s.EventId))
            }).ToList();

            return result;
        }
    }

    public async Task<object> GetReportAsync(ActivityReportInputDto input)
    {
        // 1. Get Queryables
        var eventQuery = await _eventRepository.WithDetailsAsync(x => x.Timeslots);
        var activityTypeQuery = await _activityTypeRepository.GetQueryableAsync();

        // 2. Base Query
        var query = from e in eventQuery
                    join at in activityTypeQuery on e.ActivityTypeId equals at.Id into atJoin
                    from at in atJoin.DefaultIfEmpty()
                    select new { e, at };

        // 3. Filters
        query = query.Where(x => x.e.BranchId == input.BranchId); // Branch Security

        // Date Range
        query = query.Where(x => x.e.Timeslots.Any(t => t.StartTime >= input.FromDate && t.StartTime <= input.ToDate));

        // Activity Type
        if (input.ActivityTypeId.HasValue)
        {
            query = query.Where(x => x.e.ActivityTypeId == input.ActivityTypeId);
        }

        // 4. Projection
        if (input.ReportType == ReportType.Detailed)
        {
            var list = await AsyncExecuter.ToListAsync(query);

            return list
                .OrderByDescending(x => x.e.Timeslots.Min(t => t.StartTime)) // Newest to Oldest
                .Select((x, index) => new ActivityReportDetailDto
                {
                    SerialNo = index + 1,
                    ActivityTypeName = x.at?.NameEn ?? "",
                    ActivityName = x.e.NameEn,
                    DateRange = FormatDateRange(x.e.Timeslots),
                    Location = x.e.Location
                }).ToList();
        }
        else
        {
            // Statistical
            var statsQuery = query
                .GroupBy(x => x.at != null ? x.at.NameEn : "Uncategorized")
                .Select(g => new ActivityReportStatsDto
                {
                    ActivityTypeName = g.Key,
                    Count = g.Count()
                });

            var stats = await AsyncExecuter.ToListAsync(statsQuery);
            return stats;
        }
    }

    private string FormatDateRange(IEnumerable<EventTimeslot> timeslots)
    {
        if (timeslots == null || !timeslots.Any()) return "";
        var min = timeslots.Min(t => t.StartTime);
        var max = timeslots.Max(t => t.EndTime);
        if (min.Date == max.Date)
        {
            return min.ToString("dd/MM/yyyy");
        }
        return $"{min:dd/MM/yyyy} - {max:dd/MM/yyyy}";
    }
}
