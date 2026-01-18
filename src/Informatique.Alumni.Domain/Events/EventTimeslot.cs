using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Events;

public class EventTimeslot : Entity<Guid>
{
    public Guid EventId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public int Capacity { get; private set; }

    private EventTimeslot() { }

    public EventTimeslot(Guid id, Guid eventId, DateTime startTime, DateTime endTime, int capacity)
        : base(id)
    {
        EventId = eventId;
        StartTime = startTime;
        EndTime = endTime;
        Capacity = capacity;
    }

    internal void SetEventId(Guid eventId)
    {
        EventId = eventId;
    }
}
