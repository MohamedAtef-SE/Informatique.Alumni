using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Events;

public class EventAgendaItem : Entity<Guid>
{
    public Guid EventId { get; private set; }
    public DateTime Date { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public string ActivityName { get; private set; } = string.Empty;
    public string? Place { get; private set; }
    public string? Description { get; private set; }

    private EventAgendaItem() { }

    public EventAgendaItem(
        Guid id, 
        Guid eventId, 
        DateTime date,
        TimeSpan startTime, 
        TimeSpan endTime, 
        string activityName,
        string? place = null,
        string? description = null)
        : base(id)
    {
        EventId = Check.NotDefaultOrNull<Guid>(eventId, nameof(eventId));
        Date = date;
        ActivityName = Check.NotNullOrWhiteSpace(activityName, nameof(activityName), EventConsts.MaxActivityNameLength);
        Place = place;
        Description = description;
        
        if (startTime >= endTime)
        {
            throw new BusinessException(AlumniDomainErrorCodes.EventAgendaItem.InvalidTimeRange)
                .WithData("StartTime", startTime)
                .WithData("EndTime", endTime);
        }
        
        StartTime = startTime;
        EndTime = endTime;
    }

    public void Update(
        DateTime date,
        TimeSpan startTime, 
        TimeSpan endTime, 
        string activityName,
        string? place,
        string? description)
    {
        Date = date;
        ActivityName = Check.NotNullOrWhiteSpace(activityName, nameof(activityName), EventConsts.MaxActivityNameLength);
        Place = place;
        Description = description;
        
        if (startTime >= endTime)
        {
            throw new BusinessException(AlumniDomainErrorCodes.EventAgendaItem.InvalidTimeRange)
                .WithData("StartTime", startTime)
                .WithData("EndTime", endTime);
        }
        
        StartTime = startTime;
        EndTime = endTime;
    }
}

