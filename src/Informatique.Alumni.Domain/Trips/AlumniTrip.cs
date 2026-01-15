using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Trips;

/// <summary>
/// Aggregate root for alumni trips.
/// DDD: Manages child entities (pricing tiers, requirements) lifecycle.
/// Clean Code: Encapsulation with private setters and backing fields for collections.
/// </summary>
public class AlumniTrip : FullAuditedAggregateRoot<Guid>
{
    // Core Properties
    public Guid BranchId { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public TripType Type { get; private set; }
    
    // Date Properties
    public DateTime DateFrom { get; private set; }
    public DateTime DateTo { get; private set; }
    public TimeSpan TimeFrom { get; private set; }
    public DateTime LastSubscriptionDate { get; private set; }
    public DateTime LastCancellationDate { get; private set; }
    
    // Location & Logistics
    public string Location { get; private set; } = null!;
    public decimal AdminFees { get; private set; }
    
    // Capacity Management
    public bool HasCapacityLimit { get; private set; }
    public int? CapacityLimit { get; private set; }
    
    // External Trip Properties (nullable - only for external trips)
    public string? TripProvider { get; private set; }
    public string? EmbassyRequirements { get; private set; }
    
    // Legacy properties (keeping for backward compatibility)
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public string? Destination { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public int? MaxCapacity { get; private set; }
    public decimal? PricePerPerson { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Child Collections (DDD Aggregate Pattern - Encapsulated)
    private readonly List<TripPricingTier> _pricingTiers = new();
    public IReadOnlyList<TripPricingTier> PricingTiers => _pricingTiers.AsReadOnly();
    
    private readonly List<TripRequirement> _requirements = new();
    public IReadOnlyList<TripRequirement> Requirements => _requirements.AsReadOnly();

    // EF Core constructor
    private AlumniTrip()
    {
    }

    /// <summary>
    /// Constructor for creating a new trip.
    /// SRP: Initialization only - validation happens in TripManager.
    /// </summary>
    public AlumniTrip(
        Guid id,
        Guid branchId,
        string nameAr,
        string nameEn,
        TripType type,
        DateTime dateFrom,
        DateTime dateTo,
        TimeSpan timeFrom,
        DateTime lastSubscriptionDate,
        DateTime lastCancellationDate,
        string location,
        decimal adminFees,
        bool hasCapacityLimit,
        int? capacityLimit,
        string? tripProvider = null,
        string? embassyRequirements = null) : base(id)
    {
        BranchId = branchId;
        SetNames(nameAr, nameEn);
        Type = type;
        SetDates(dateFrom, dateTo, timeFrom, lastSubscriptionDate, lastCancellationDate);
        Location = Check.NotNullOrWhiteSpace(location, nameof(location), TripConsts.MaxLocationLength);
        AdminFees = adminFees;
        SetCapacity(hasCapacityLimit, capacityLimit);
        SetExternalTripFields(tripProvider, embassyRequirements);
        
        // Set legacy properties for compatibility
        Title = nameEn;
        Destination = location;
        StartDate = dateFrom;
        EndDate = dateTo;
        MaxCapacity = capacityLimit;
    }

    /// <summary>
    /// Sets the trip names.
    /// Clean Code: Focused validation.
    /// </summary>
    public void SetNames(string nameAr, string nameEn)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr), TripConsts.MaxTitleLength);
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn), TripConsts.MaxTitleLength);
    }

    /// <summary>
    /// Sets trip dates with validation.
    /// Business Rule: EndDate > StartDate, Deadlines < StartDate.
    /// Clean Code: Single responsibility method.
    /// </summary>
    public void SetDates(
        DateTime dateFrom,
        DateTime dateTo,
        TimeSpan timeFrom,
        DateTime lastSubscriptionDate,
        DateTime lastCancellationDate)
    {
        if (dateTo <= dateFrom)
        {
            throw new BusinessException("Trip:InvalidDateRange")
                .WithData("DateFrom", dateFrom)
                .WithData("DateTo", dateTo);
        }

        if (lastSubscriptionDate >= dateFrom)
        {
            throw new BusinessException("Trip:LastSubscriptionDateMustBeBeforeStart")
                .WithData("LastSubscriptionDate", lastSubscriptionDate)
                .WithData("DateFrom", dateFrom);
        }

        if (lastCancellationDate >= dateFrom)
        {
            throw new BusinessException("Trip:LastCancellationDateMustBeBeforeStart")
                .WithData("LastCancellationDate", lastCancellationDate)
                .WithData("DateFrom", dateFrom);
        }

        DateFrom = dateFrom;
        DateTo = dateTo;
        TimeFrom = timeFrom;
        LastSubscriptionDate = lastSubscriptionDate;
        LastCancellationDate = lastCancellationDate;
    }

    /// <summary>
    /// Sets capacity information.
    /// Business Rule: If capacity is enabled, limit must be positive.
    /// </summary>
    public void SetCapacity(bool hasCapacityLimit, int? capacityLimit)
    {
        if (hasCapacityLimit && (!capacityLimit.HasValue || capacityLimit.Value <= 0))
        {
            throw new BusinessException("Trip:CapacityLimitRequired")
                .WithData("HasCapacityLimit", hasCapacityLimit)
                .WithData("CapacityLimit", capacityLimit);
        }

        HasCapacityLimit = hasCapacityLimit;
        CapacityLimit = capacityLimit;
    }

    /// <summary>
    /// Sets external trip specific fields.
    /// Business Rule: External trips must have provider and embassy requirements.
    /// </summary>
    public void SetExternalTripFields(string? tripProvider, string? embassyRequirements)
    {
        TripProvider = tripProvider;
        EmbassyRequirements = embassyRequirements;
    }

    /// <summary>
    /// Adds a pricing tier to the trip.
    /// DDD: Aggregate root controls child entity creation.
    /// Business Rule: Age ranges for same room type cannot overlap.
    /// </summary>
    public void AddPricingTier(TripPricingTier tier)
    {
        Check.NotNull(tier, nameof(tier));

        // Business Rule: Check for age range overlap with existing tiers for same room type
        var existingTiersForRoom = _pricingTiers.Where(t => t.RoomTypeId == tier.RoomTypeId);
        foreach (var existingTier in existingTiersForRoom)
        {
            if (existingTier.OverlapsWith(tier))
            {
                throw new BusinessException("Trip:AgeRangeOverlap")
                    .WithData("RoomTypeId", tier.RoomTypeId)
                    .WithData("ExistingFrom", existingTier.ChildAgeFrom)
                    .WithData("ExistingTo", existingTier.ChildAgeTo)
                    .WithData("NewFrom", tier.ChildAgeFrom)
                    .WithData("NewTo", tier.ChildAgeTo);
            }
        }

        _pricingTiers.Add(tier);
    }

    /// <summary>
    /// Adds a document requirement to the trip.
    /// DDD: Aggregate root manages child lifecycle.
    /// </summary>
    public void AddRequirement(TripRequirement requirement)
    {
        Check.NotNull(requirement, nameof(requirement));
        _requirements.Add(requirement);
    }

    /// <summary>
    /// Removes all pricing tiers (for updates).
    /// DDD: Only aggregate root can modify children.
    /// </summary>
    public void ClearPricingTiers()
    {
        _pricingTiers.Clear();
    }

    /// <summary>
    /// Removes all requirements (for updates).
    /// </summary>
    public void ClearRequirements()
    {
        _requirements.Clear();
    }

    // Legacy methods (keeping for backward compatibility)
    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
