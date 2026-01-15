using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Trips;

/// <summary>
/// Child entity for trip pricing tiers.
/// DDD: Owned entity, part of AlumniTrip aggregate.
/// Clean Code: Encapsulation with private setters, clear validation.
/// </summary>
public class TripPricingTier : Entity<Guid>
{
    /// <summary>
    /// Foreign key to the owning trip.
    /// DDD: Aggregate relationship.
    /// </summary>
    public Guid TripId { get; private set; }
    
    /// <summary>
    /// Foreign key to the room type.
    /// Business Rule: Each tier is for a specific room type.
    /// </summary>
    public Guid RoomTypeId { get; private set; }
    
    /// <summary>
    /// Price for association members.
    /// </summary>
    public decimal MemberPrice { get; private set; }
    
    /// <summary>
    /// Price for companions (non-members).
    /// </summary>
    public decimal CompanionPrice { get; private set; }
    
    /// <summary>
    /// Price for children in this age range.
    /// </summary>
    public decimal ChildPrice { get; private set; }
    
    /// <summary>
    /// Minimum age for this child pricing tier (inclusive).
    /// Business Rule: Part of age range definition.
    /// </summary>
    public int ChildAgeFrom { get; private set; }
    
    /// <summary>
    /// Maximum age for this child pricing tier (inclusive).
    /// Business Rule: Part of age range definition.
    /// </summary>
    public int ChildAgeTo { get; private set; }
    
    /// <summary>
    /// Whether children in this age range count towards room capacity.
    /// Example: Infants (0-2) may not count, but kids (6-12) do.
    /// </summary>
    public bool CountsTowardsRoomCapacity { get; private set; }

    // EF Core constructor
    private TripPricingTier()
    {
    }

    /// <summary>
    /// Constructor for creating a new pricing tier.
    /// SRP: Focuses on initialization and basic validation.
    /// </summary>
    public TripPricingTier(
        Guid id,
        Guid tripId,
        Guid roomTypeId,
        decimal memberPrice,
        decimal companionPrice,
        decimal childPrice,
        int childAgeFrom,
        int childAgeTo,
        bool countsTowardsRoomCapacity) : base(id)
    {
        TripId = tripId;
        RoomTypeId = roomTypeId;
        
        SetPrices(memberPrice, companionPrice, childPrice);
        SetAgeRange(childAgeFrom, childAgeTo);
        
       CountsTowardsRoomCapacity = countsTowardsRoomCapacity;
    }

    /// <summary>
    /// Sets the prices for this tier.
    /// Business Rule: Prices cannot be negative.
    /// Clean Code: Focused validation method.
    /// </summary>
    public void SetPrices(decimal memberPrice, decimal companionPrice, decimal childPrice)
    {
        if (memberPrice < 0)
            throw new ArgumentException("Member price cannot be negative", nameof(memberPrice));
        if (companionPrice < 0)
            throw new ArgumentException("Companion price cannot be negative", nameof(companionPrice));
        if (childPrice < 0)
            throw new ArgumentException("Child price cannot be negative", nameof(childPrice));
            
        MemberPrice = memberPrice;
        CompanionPrice = companionPrice;
        ChildPrice = childPrice;
    }

    /// <summary>
    /// Sets the age range for child pricing.
    /// Business Rule: From age must be less than To age.
    /// </summary>
    public void SetAgeRange(int fromAge, int toAge)
    {
        if (fromAge < 0)
            throw new ArgumentException("Age cannot be negative", nameof(fromAge));
        if (toAge < fromAge)
            throw new ArgumentException("To age must be greater than or equal to from age", nameof(toAge));
            
        ChildAgeFrom = fromAge;
        ChildAgeTo = toAge;
    }

    /// <summary>
    /// Checks if this tier's age range overlaps with another.
    /// Business Rule: Age ranges for the same room type cannot overlap.
    /// Clean Code: Single responsibility - overlap detection.
    /// </summary>
    public bool OverlapsWith(TripPricingTier other)
    {
        return ChildAgeFrom <= other.ChildAgeTo && other.ChildAgeFrom <= ChildAgeTo;
    }
}
