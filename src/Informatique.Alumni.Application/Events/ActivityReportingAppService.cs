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
    private readonly IRepository<IdentityUser, Guid> _userRepository; // Use IRepository instead of IIdentityUserRepository for GetQueryableAsync
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IRepository<College, Guid> _collegeRepository;

    public ActivityReportingAppService(
        IRepository<AlumniEventRegistration, Guid> registrationRepository,
        IRepository<AssociationEvent, Guid> eventRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<Education, Guid> educationRepository,
        IRepository<ActivityType, Guid> activityTypeRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<Branch, Guid> branchRepository,
        IRepository<College, Guid> collegeRepository)
    {
        _registrationRepository = registrationRepository;
        _eventRepository = eventRepository;
        _profileRepository = profileRepository;
        _educationRepository = educationRepository;
        _activityTypeRepository = activityTypeRepository;
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _collegeRepository = collegeRepository;
    }

    public async Task<object> GetParticipantsReportAsync(ParticipantReportInputDto input)
    {
        var registrationQuery = await _registrationRepository.GetQueryableAsync();
        var eventQuery = await _eventRepository.WithDetailsAsync(x => x.Timeslots);
        var profileQuery = await _profileRepository.GetQueryableAsync();
        var educationQuery = await _educationRepository.GetQueryableAsync();
        var activityTypeQuery = await _activityTypeRepository.GetQueryableAsync();
        var userQuery = await _userRepository.GetQueryableAsync();
        var branchQuery = await _branchRepository.GetQueryableAsync();

        var query = from r in registrationQuery
                    join e in eventQuery on r.EventId equals e.Id
                    join p in profileQuery on r.AlumniId equals p.UserId
                    join u in userQuery on r.AlumniId equals u.Id
                    join b in branchQuery on e.BranchId equals b.Id
                    join at in activityTypeQuery on e.ActivityTypeId equals at.Id into atJoin
                    from at in atJoin.DefaultIfEmpty()
                    where r.Status != RegistrationStatus.Cancelled 
                    select new { r, e, p, u, b, at };

        // Simple filtering for now
        query = query.Where(x => x.e.BranchId == input.BranchId);

        var list = await AsyncExecuter.ToListAsync(query);
        return list;
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

// Inline DTO to satisfy build for now if it's missing in contracts
public class ParticipantReportInputDto
{
    public Guid BranchId { get; set; }
    public Guid? ActivityTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}
