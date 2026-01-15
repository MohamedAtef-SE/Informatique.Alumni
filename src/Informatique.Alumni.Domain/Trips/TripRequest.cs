using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Trips;

public class TripRequest : FullAuditedAggregateRoot<Guid>
{
    public Guid TripId { get; set; }
    public Guid AlumniId { get; set; }
    public int GuestCount { get; set; }
    public decimal TotalAmount { get; set; }
    public TripRequestStatus Status { get; set; }

    public int TotalParticipants => 1 + GuestCount;

    private TripRequest() { }

    public TripRequest(Guid id, Guid tripId, Guid alumniId, int guestCount, decimal totalAmount) : base(id)
    {
        TripId = tripId;
        AlumniId = alumniId;
        GuestCount = guestCount;
        TotalAmount = totalAmount;
        Status = TripRequestStatus.Pending;
    }
}
