using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Events;

public class ActivityManager : DomainService
{
    private readonly IRepository<AssociationEvent, Guid> _eventRepository;

    public ActivityManager(IRepository<AssociationEvent, Guid> eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<AssociationEvent> CreateAsync(
        string nameAr,
        string nameEn,
        string code,
        string description,
        string location,
        string? address,
        string? googleMapUrl,
        bool hasFees,
        decimal? feeAmount,
        DateTime lastSubscriptionDate,
        Guid? branchId,
        List<(DateTime Start, DateTime End, int Capacity)> timeslots,
        List<(Guid CompanyId, Guid ParticipationTypeId)> participatingCompanies)
    {
        // 1. Create Core Entity (Validates Fees internally via SetFees)
        var @event = new AssociationEvent(
            GuidGenerator.Create(),
            nameAr,
            nameEn,
            code,
            description,
            location,
            address,
            googleMapUrl,
            hasFees,
            feeAmount,
            lastSubscriptionDate,
            branchId
        );

        // 2. Validate & Add Timeslots
        if (timeslots == null || !timeslots.Any())
        {
            throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.NoTimeslotsDefined);
        }

        foreach (var slot in timeslots)
        {
            @event.AddTimeslot(GuidGenerator.Create(), slot.Start, slot.End, slot.Capacity);
        }
        
        // 3. Validate LastSubscriptionDate Rule
        // Rule: LastSubscriptionDate MUST be <= the earliest EventTimeslot.StartTime
        var earliestStart = timeslots.Min(x => x.Start);
        if (lastSubscriptionDate > earliestStart)
        {
             throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.InvalidSubscriptionDate)
                 .WithData("LastSubscriptionDate", lastSubscriptionDate)
                 .WithData("EarliestTimeslot", earliestStart);
        }

        // 4. Add Participating Companies
        if (participatingCompanies != null)
        {
            foreach (var company in participatingCompanies)
            {
                @event.AddParticipatingCompany(GuidGenerator.Create(), company.CompanyId, company.ParticipationTypeId);
            }
        }

        return await _eventRepository.InsertAsync(@event);
    }


    public async Task UpdateAsync(
        AssociationEvent @event,
        string nameAr,
        string nameEn,
        string code,
        string description,
        string location,
        string? address,
        string? googleMapUrl,
        bool hasFees,
        decimal? feeAmount,
        DateTime lastSubscriptionDate,
        Guid? branchId,
        List<(DateTime Start, DateTime End, int Capacity)> timeslots,
        List<(Guid CompanyId, Guid ParticipationTypeId)> participatingCompanies)
    {
        // 1. Update Core Properties
        @event.Update(
            nameAr,
            nameEn,
            code,
            description,
            location,
            address,
            googleMapUrl,
            hasFees,
            feeAmount,
            lastSubscriptionDate,
            branchId
        );

        // 2. Update Timeslots (Clear & Re-Add Strategy)
        if (timeslots == null || !timeslots.Any())
        {
            throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.NoTimeslotsDefined);
        }

        @event.ClearTimeslots();
        foreach (var slot in timeslots)
        {
            @event.AddTimeslot(GuidGenerator.Create(), slot.Start, slot.End, slot.Capacity);
        }

        // Validate LastSubscriptionDate Rule again
        var earliestStart = timeslots.Min(x => x.Start);
        if (lastSubscriptionDate > earliestStart)
        {
             throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.InvalidSubscriptionDate)
                 .WithData("LastSubscriptionDate", lastSubscriptionDate)
                 .WithData("EarliestTimeslot", earliestStart);
        }

        // 3. Update Participating Companies
        @event.ClearParticipatingCompanies();
        if (participatingCompanies != null)
        {
            foreach (var company in participatingCompanies)
            {
                @event.AddParticipatingCompany(GuidGenerator.Create(), company.CompanyId, company.ParticipationTypeId);
            }
        }


        await _eventRepository.UpdateAsync(@event);
    }

    /// <summary>
    /// Adds an agenda item to an event with conflict detection.
    /// Business Rule: Prevent double-booking of locations at the same time.
    /// </summary>
    public async Task<EventAgendaItem> AddAgendaItemAsync(
        AssociationEvent @event,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        string activityName,
        string? place = null,
        string? description = null)
    {
        // 1. Validate Time Range (StartTime < EndTime)
        if (startTime >= endTime)
        {
            throw new BusinessException(AlumniDomainErrorCodes.EventAgendaItem.InvalidTimeRange)
                .WithData("StartTime", startTime)
                .WithData("EndTime", endTime);
        }

        // 2. Conflict Detection: Check for overlapping items in the same location
        if (!string.IsNullOrWhiteSpace(place))
        {
            // Get existing agenda items for this event in the same place
            var existingItems = @event.AgendaItems
                .Where(x => x.Date.Date == date.Date && 
                           !string.IsNullOrWhiteSpace(x.Place) && 
                           x.Place.Equals(place, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Check for time overlap: (newStart < existingEnd) && (newEnd > existingStart)
            foreach (var existing in existingItems)
            {
                bool hasOverlap = (startTime < existing.EndTime) && (endTime > existing.StartTime);
                
                if (hasOverlap)
                {
                    throw new BusinessException(AlumniDomainErrorCodes.EventAgendaItem.ScheduleConflict)
                        .WithData("Place", place)
                        .WithData("Date", date)
                        .WithData("RequestedStartTime", startTime)
                        .WithData("RequestedEndTime", endTime)
                        .WithData("ConflictingStartTime", existing.StartTime)
                        .WithData("ConflictingEndTime", existing.EndTime)
                        .WithData("ConflictingActivity", existing.ActivityName);
                }
            }
        }

        // 3. Create and add the agenda item
        var agendaItem = new EventAgendaItem(
            GuidGenerator.Create(),
            @event.Id,
            date,
            startTime,
            endTime,
            activityName,
            place,
            description);

        @event.AddAgendaItem(agendaItem);

        return agendaItem;
    }
}
