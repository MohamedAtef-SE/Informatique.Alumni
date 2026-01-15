using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Trips;

/// <summary>
/// Child entity for trip subscription attendees.
/// DDD: Owned entity, part of TripSubscription aggregate.
/// Clean Code: Encapsulation with private setters, price snapshot pattern.
/// </summary>
public class TripSubscriptionAttendee : Entity<Guid>
{
    /// <summary>
    /// Foreign key to the owning subscription.
    /// DDD: Aggregate relationship.
    /// </summary>
    public Guid TripSubscriptionId { get; private set; }
    
    /// <summary>
    /// Full name of the attendee.
    /// </summary>
    public string Name { get; private set; } = null!;
    
    /// <summary>
    /// Type of attendee (Member, Companion, Child).
    /// Business Rule: Determines pricing tier.
    /// </summary>
    public AttendeeType Type { get; private set; }
    
    /// <summary>
    /// Age of the attendee (required for children pricing).
    /// Nullable for members/companions where age is not required.
    /// </summary>
    public int? Age { get; private set; }
    
    /// <summary>
    /// Gender of the attendee.
    /// Optional demographic information.
    /// </summary>
    public Gender? Gender { get; private set; }
    
    /// <summary>
    /// Relationship to the member (for companions).
    /// Only applicable when Type = Companion.
    /// </summary>
    public CompanionRelation? Relation { get; private set; }
    
    /// <summary>
    /// Price snapshot at time of booking.
    /// Business Rule: Protects against price changes after subscription.
    /// Audit Trail: Records historical pricing.
    /// </summary>
    public decimal PriceSnapshot { get; private set; }
    
    /// <summary>
    /// Whether this attendee counts towards room capacity.
    /// Business Rule: Infants may not count, but older children do.
    /// Derived from pricing tier at booking time.
    /// </summary>
    public bool CountsTowardsCapacity { get; private set; }

    // EF Core constructor
    private TripSubscriptionAttendee()
    {
    }

    /// <summary>
    /// Constructor for creating a new attendee.
    /// SRP: Initialization only, validation in aggregate root.
    /// </summary>
    public TripSubscriptionAttendee(
        Guid id,
        Guid tripSubscriptionId,
        string name,
        AttendeeType type,
        decimal priceSnapshot,
        bool countsTowardsCapacity,
        int? age = null,
        Gender? gender = null,
        CompanionRelation? relation = null) : base(id)
    {
        TripSubscriptionId = tripSubscriptionId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 256);
        Type = type;
        PriceSnapshot = priceSnapshot;
        CountsTowardsCapacity = countsTowardsCapacity;
        Age = age;
        Gender = gender;
        Relation = relation;
        
        // Business Rule: Children must have age
        if (type == AttendeeType.Child && !age.HasValue)
        {
            throw new ArgumentException("Age is required for child attendees", nameof(age));
        }
        
        // Business Rule: Companions should have relation
        if (type == AttendeeType.Companion && !relation.HasValue)
        {
            throw new ArgumentException("Relation is required for companion attendees", nameof(relation));
        }
    }
}
