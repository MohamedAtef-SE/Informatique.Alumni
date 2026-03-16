using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Trips;

public class TripRequest : FullAuditedAggregateRoot<Guid>
{
    public Guid TripId { get; private set; }
    public Guid AlumniId { get; private set; }
    public int GuestCount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public TripRequestStatus Status { get; private set; }

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

    public void Approve()
    {
        if (Status != TripRequestStatus.Pending)
        {
            throw new BusinessException("Trip:CannotApproveNonPendingRequest")
                .WithData("CurrentStatus", Status);
        }
        Status = TripRequestStatus.Approved;
    }

    public void Reject()
    {
        if (Status != TripRequestStatus.Pending)
        {
            throw new BusinessException("Trip:CannotRejectNonPendingRequest")
                .WithData("CurrentStatus", Status);
        }
        Status = TripRequestStatus.Rejected;
    }

    public void Cancel()
    {
        if (Status == TripRequestStatus.Cancelled)
        {
            throw new BusinessException("Trip:AlreadyCancelled");
        }
        Status = TripRequestStatus.Cancelled;
    }
}

