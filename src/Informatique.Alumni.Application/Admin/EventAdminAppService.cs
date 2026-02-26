using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Events;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.EventManage)]
public class EventAdminAppService : AlumniAppService, IEventAdminAppService
{
    private readonly IRepository<AssociationEvent, Guid> _eventRepository;
    private readonly IRepository<AlumniEventRegistration, Guid> _registrationRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IIdentityUserRepository _userRepository;

    private readonly ActivityManager _activityManager;
    private readonly AlumniApplicationMappers _alumniMappers;

    public EventAdminAppService(
        IRepository<AssociationEvent, Guid> eventRepository,
        IRepository<AlumniEventRegistration, Guid> registrationRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IIdentityUserRepository userRepository,
        ActivityManager activityManager,
        AlumniApplicationMappers alumniMappers)
    {
        _eventRepository = eventRepository;
        _registrationRepository = registrationRepository;
        _profileRepository = profileRepository;
        _userRepository = userRepository;
        _activityManager = activityManager;
        _alumniMappers = alumniMappers;
    }

    public async Task<PagedResultDto<EventAdminDto>> GetListAsync(EventAdminGetListInput input)
    {
        var queryable = await _eventRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x =>
                x.NameAr.Contains(input.Filter) ||
                x.NameEn.Contains(input.Filter) ||
                x.Code.Contains(input.Filter));
        }

        var totalCount = queryable.Count();

        queryable = queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var events = queryable.ToList();
        
        // Optimizing count query
        var eventIds = events.Select(e => e.Id).ToList();
        var regCounts = await AsyncExecuter.ToListAsync(
            (await _registrationRepository.GetQueryableAsync())
            .Where(r => eventIds.Contains(r.EventId))
            .GroupBy(r => r.EventId)
            .Select(g => new { EventId = g.Key, Count = g.Count() })
        );

        var items = events.Select(e =>
        {
            var firstTimeslot = e.Timeslots.OrderBy(t => t.StartTime).FirstOrDefault();

            return new EventAdminDto
            {
                Id = e.Id,
                NameAr = e.NameAr,
                NameEn = e.NameEn,
                Code = e.Code,
                Location = e.Location,
                StartDate = firstTimeslot?.StartTime ?? DateTime.MinValue,
                EndDate = firstTimeslot?.EndTime ?? DateTime.MinValue,
                LastSubscriptionDate = e.LastSubscriptionDate,
                IsPublished = e.IsPublished,
                IsFree = !e.HasFees,
                FeeAmount = e.FeeAmount,
                RegistrationCount = regCounts.FirstOrDefault(c => c.EventId == e.Id)?.Count ?? 0,
                CreationTime = e.CreationTime
            };
        }).ToList();

        return new PagedResultDto<EventAdminDto>(totalCount, items);
    }

    public async Task<Informatique.Alumni.Events.AssociationEventDto> GetAsync(Guid id)
    {
        var e = await _eventRepository.GetAsync(id);
        await _eventRepository.EnsureCollectionLoadedAsync(e, x => x.Timeslots);
        await _eventRepository.EnsureCollectionLoadedAsync(e, x => x.ParticipatingCompanies);

        return _alumniMappers.MapToDto(e);
    }

    public async Task<PagedResultDto<EventAttendeeDto>> GetAttendeesAsync(
        Guid eventId, PagedAndSortedResultRequestDto input)
    {
        var queryable = (await _registrationRepository.GetQueryableAsync())
            .Where(r => r.EventId == eventId);

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        queryable = queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var registrations = await AsyncExecuter.ToListAsync(queryable);

        // Resolve Names
        var alumniIds = registrations.Select(r => r.AlumniId).Distinct().ToList();
        var profiles = await _profileRepository.GetListAsync(p => alumniIds.Contains(p.Id));
        var userIds = profiles.Select(p => p.UserId).Distinct().ToList();
        var users = await _userRepository.GetListByIdsAsync(userIds);

        var items = registrations.Select(r =>
        {
            var profile = profiles.FirstOrDefault(p => p.Id == r.AlumniId);
            var user = profile != null ? users.FirstOrDefault(u => u.Id == profile.UserId) : null;

            return new EventAttendeeDto
            {
                Id = r.Id,
                AlumniId = r.AlumniId,
                AlumniName = user != null ? $"{user.Name} {user.Surname}" : "Unknown",
                TicketCode = r.TicketCode,
                CreationTime = r.CreationTime
            };
        }).ToList();

        return new PagedResultDto<EventAttendeeDto>(totalCount, items);
    }

    public async Task<AssociationEventDto> CreateEventAsync(CreateEventDto input)
    {
        var timeslots = input.Timeslots.Select(x => (x.StartTime, x.EndTime, x.Capacity)).ToList();
        var companies = input.ParticipatingCompanies.Select(x => (x.CompanyId, x.ParticipationTypeId)).ToList();

        var @event = await _activityManager.CreateAsync(
            input.NameAr,
            input.NameEn,
            input.Code,
            input.Description,
            input.Location,
            input.Address,
            input.GoogleMapUrl,
            input.HasFees,
            input.FeeAmount,
            input.LastSubscriptionDate,
            input.BranchId,
            timeslots,
            companies
        );

        return _alumniMappers.MapToDto(@event);
    }

    public async Task<AssociationEventDto> UpdateEventAsync(Guid id, UpdateEventDto input)
    {
        var @event = await _eventRepository.GetAsync(id);
        
        await _eventRepository.EnsureCollectionLoadedAsync(@event, x => x.Timeslots);
        await _eventRepository.EnsureCollectionLoadedAsync(@event, x => x.ParticipatingCompanies);

        var timeslots = input.Timeslots.Select(x => (x.StartTime, x.EndTime, x.Capacity)).ToList();
        var companies = input.ParticipatingCompanies.Select(x => (x.CompanyId, x.ParticipationTypeId)).ToList();

        await _activityManager.UpdateAsync(
            @event,
            input.NameAr,
            input.NameEn,
            input.Code,
            input.Description,
            input.Location,
            input.Address,
            input.GoogleMapUrl,
            input.HasFees,
            input.FeeAmount,
            input.LastSubscriptionDate,
            input.BranchId,
            timeslots,
            companies
        );

        return _alumniMappers.MapToDto(@event);
    }

    public async Task DeleteEventAsync(Guid id)
    {
        await _eventRepository.DeleteAsync(id);
    }

    public async Task PublishEventAsync(Guid id)
    {
        var @event = await _eventRepository.GetAsync(id);
        await _eventRepository.EnsureCollectionLoadedAsync(@event, x => x.Timeslots);
        
        @event.Publish();
        await _eventRepository.UpdateAsync(@event);
    }
}
