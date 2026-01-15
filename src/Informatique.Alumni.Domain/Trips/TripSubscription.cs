using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Trips;

/// <summary>
/// Aggregate root for trip subscriptions.
/// DDD: Manages attendees lifecycle, financial state, and business rules.
/// Clean Code: Encapsulation, price snapshot pattern for audit trail.
/// </summary>
public class TripSubscription : AuditedAggregateRoot<Guid>
{
    /// <summary>
    /// Foreign key to the trip.
    /// </summary>
    public Guid TripId { get; private set; }
    
    /// <summary>
    /// Foreign key to the subscribing alumni.
    /// </summary>
    public Guid AlumniId { get; private set; }
    
    /// <summary>
    /// Selected room type for this subscription.
    /// Business Rule: Determines pricing tier.
    /// </summary>
    public Guid RoomTypeId { get; private set; }
    
    /// <summary>
    /// Current status of the subscription.
    /// </summary>
    public TripSubscriptionStatus Status { get; private set; }
    
    // Financial Properties
    /// <summary>
    /// Total amount calculated at booking time.
    /// Formula: MemberPrice + CompanionPrices + ChildPrices + AdminFees
    /// </summary>
    public decimal TotalAmount { get; private set; }
    
    /// <summary>
    /// Admin fee snapshot at booking time.
    /// Business Rule: Admin fees are NEVER refunded.
    /// Audit Trail: Records fee structure at time of booking.
    /// </summary>
    public decimal AdminFeeSnapshot { get; private set; }
    
    /// <summary>
    /// Amount actually paid by the user.
    /// May equal TotalAmount or less if partially paid.
    /// </summary>
    public decimal PaidAmount { get; private set; }
    
    /// <summary>
    /// Payment method used for this subscription.
    /// </summary>
    public PaymentMethod PaymentMethod { get; private set; }
    
    /// <summary>
    /// Payment gateway transaction ID (if applicable).
    /// Nullable for wallet-only payments.
    /// </summary>
    public string? GatewayTransactionId { get; private set; }

    // Child Collection (DDD Aggregate Pattern)
    private readonly List<TripSubscriptionAttendee> _attendees = new();
    public IReadOnlyList<TripSubscriptionAttendee> Attendees => _attendees.AsReadOnly();

    // EF Core constructor
    private TripSubscription()
    {
    }

    /// <summary>
    /// Constructor for creating a new subscription.
    /// SRP: Initialization only - complex validation happens in TripManager.
    /// </summary>
    public TripSubscription(
        Guid id,
        Guid tripId,
        Guid alumniId,
        Guid roomTypeId,
        decimal totalAmount,
        decimal adminFeeSnapshot) : base(id)
    {
        TripId = tripId;
        AlumniId = alumniId;
        RoomTypeId = roomTypeId;
        TotalAmount = totalAmount;
        AdminFeeSnapshot = adminFeeSnapshot;
        Status = TripSubscriptionStatus.Pending;
        PaidAmount = 0;
    }

    /// <summary>
    /// Adds an attendee to the subscription.
    /// DDD: Aggregate root controls child entity creation.
    /// </summary>
    public void AddAttendee(TripSubscriptionAttendee attendee)
    {
        Check.NotNull(attendee, nameof(attendee));
        
        // Business Rule: Cannot modify after payment
        if (Status == TripSubscriptionStatus.Active)
        {
            throw new BusinessException("TripSubscription:CannotModifyActiveSubscription");
        }
        
        _attendees.Add(attendee);
    }

    /// <summary>
    /// Marks the subscription as paid and activates it.
    /// Business Rule: Status transitions from Pending → Active.
    /// </summary>
    public void MarkAsPaid(decimal amount, PaymentMethod paymentMethod, string? gatewayTransactionId = null)
    {
        if (Status != TripSubscriptionStatus.Pending)
        {
            throw new BusinessException("TripSubscription:InvalidStatusForPayment")
                .WithData("CurrentStatus", Status);
        }
        
        if (amount < TotalAmount)
        {
            throw new BusinessException("TripSubscription:InsufficientPayment")
                .WithData("Required", TotalAmount)
                .WithData("Provided", amount);
        }
        
        PaidAmount = amount;
        PaymentMethod = paymentMethod;
        GatewayTransactionId = gatewayTransactionId;
        Status = TripSubscriptionStatus.Active;
    }

    /// <summary>
    /// Cancels the subscription.
    /// Business Rule: Can only cancel Active subscriptions.
    /// Clean Code: Focused method, single responsibility.
    /// </summary>
    public void Cancel()
    {
        if (Status != TripSubscriptionStatus.Active)
        {
            throw new BusinessException("TripSubscription:CanOnlyCancelActiveSubscription")
                .WithData("CurrentStatus", Status);
        }
        
        Status = TripSubscriptionStatus.Cancelled;
    }

    /// <summary>
    /// Calculates refund amount based on business rules.
    /// Business Rule: RefundAmount = TotalPaid - AdminFees (admin fees NEVER refunded).
    /// Clean Code: Encapsulates refund calculation logic.
    /// </summary>
    public decimal CalculateRefundAmount()
    {
        if (Status != TripSubscriptionStatus.Cancelled)
        {
            throw new BusinessException("TripSubscription:CanOnlyRefundCancelledSubscription");
        }
        
        // Business Rule: Admin fees are never refunded
        var refundAmount = PaidAmount - AdminFeeSnapshot;
        
        return refundAmount > 0 ? refundAmount : 0;
    }

    /// <summary>
    /// Gets the count of attendees that count towards capacity.
    /// Business Rule: Used for capacity management.
    /// </summary>
    public int GetCapacityCount()
    {
        return _attendees.Count(a => a.CountsTowardsCapacity);
    }
}
