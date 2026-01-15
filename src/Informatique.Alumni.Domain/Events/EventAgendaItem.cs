using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Events;

public class EventAgendaItem : Entity<Guid>
{
    public Guid EventId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string Speaker { get; private set; } = string.Empty;

    private EventAgendaItem() { }

    public EventAgendaItem(Guid id, Guid eventId, string title, DateTime startTime, DateTime endTime, string speaker)
        : base(id)
    {
        EventId = Check.NotDefaultOrNull<Guid>(eventId, nameof(eventId));
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), EventConsts.MaxAgendaTitleLength);
        Speaker = Check.NotNullOrWhiteSpace(speaker, nameof(speaker), EventConsts.MaxSpeakerLength);
        
        if (startTime >= endTime)
        {
            throw new BusinessException(AlumniDomainErrorCodes.EventAgendaItem.InvalidTimeRange)
                .WithData("StartTime", startTime)
                .WithData("EndTime", endTime);
        }
        
        StartTime = startTime;
        EndTime = endTime;
    }

    public void Update(string title, DateTime startTime, DateTime endTime, string speaker)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), EventConsts.MaxAgendaTitleLength);
        Speaker = Check.NotNullOrWhiteSpace(speaker, nameof(speaker), EventConsts.MaxSpeakerLength);
        
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

