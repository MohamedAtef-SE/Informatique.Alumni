using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Events;

public class EventParticipatingCompany : Entity<Guid>
{
    public Guid EventId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid ParticipationTypeId { get; private set; }

    public virtual Company? Company { get; private set; }
    public virtual ParticipationType? ParticipationType { get; private set; }

    private EventParticipatingCompany() { }

    public EventParticipatingCompany(Guid id, Guid eventId, Guid companyId, Guid participationTypeId)
        : base(id)
    {
        EventId = eventId;
        CompanyId = companyId;
        ParticipationTypeId = participationTypeId;
    }
    
    internal void SetEventId(Guid eventId)
    {
        EventId = eventId;
    }
}
