using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Trips;

/// <summary>
/// Domain service for trip creation and validation.
/// DDD: Coordinates complex business logic that spans multiple entities.
/// SRP: Each validation method has single responsibility.
/// Clean Code: Small methods (< 15 lines), clear naming.
/// </summary>
public class TripManager : DomainService
{
    private readonly IRepository<AlumniTrip, Guid> _tripRepository;
    private readonly IRepository<TripSubscription, Guid> _subscriptionRepository;

    public TripManager(
        IRepository<AlumniTrip, Guid> tripRepository,
        IRepository<TripSubscription, Guid> subscriptionRepository)
    {
        _tripRepository = tripRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    /// <summary>
    /// Creates a new trip with complete validation.
    /// SRP: Orchestrates validation, then creates entity.
    /// Business Rules: Uniqueness, date logic, external trip fields, age ranges.
    /// </summary>
    public async Task<AlumniTrip> CreateTripAsync(
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
        string? tripProvider,
        string? embassyRequirements,
        List<CreateTripPricingTierInput> pricingTiers,
        List<CreateTripRequirementInput> requirements)
    {
        // 1. Validate Uniqueness (DateFrom + TimeFrom + Location)
        await CheckUniquenessAsync(dateFrom, timeFrom, location);

        // 2. Validate External Trip Fields
        ValidateExternalTripFields(type, tripProvider, embassyRequirements);

        // 3. Validate Age Range Overlaps (per RoomType)
        ValidateAgeRangeOverlaps(pricingTiers);

        // 4. Create Trip Aggregate Root
        var trip = new AlumniTrip(
            GuidGenerator.Create(),
            branchId,
            nameAr,
            nameEn,
            type,
            dateFrom,
            dateTo,
            timeFrom,
            lastSubscriptionDate,
            lastCancellationDate,
            location,
            adminFees,
            hasCapacityLimit,
            capacityLimit,
            tripProvider,
            embassyRequirements
        );

        // 5. Add Pricing Tiers (Aggregate handles overlap validation)
        foreach (var tierInput in pricingTiers)
        {
            var tier = new TripPricingTier(
                GuidGenerator.Create(),
                trip.Id,
                tierInput.RoomTypeId,
                tierInput.MemberPrice,
                tierInput.CompanionPrice,
                tierInput.ChildPrice,
                tierInput.ChildAgeFrom,
                tierInput.ChildAgeTo,
                tierInput.CountsTowardsRoomCapacity
            );

            trip.AddPricingTier(tier);
        }

        // 6. Add Requirements
        foreach (var reqInput in requirements)
        {
            var requirement = new TripRequirement(
                GuidGenerator.Create(),
                trip.Id,
                reqInput.DocumentId,
                reqInput.TargetAudience,
                reqInput.SubmissionType
            );

            trip.AddRequirement(requirement);
        }

        return trip;
    }

    /// <summary>
    /// Validates trip uniqueness based on execution details.
    /// Business Rule: NOT by name, but by DateFrom + TimeFrom + Location.
    /// Clean Code: Single responsibility - uniqueness check only.
    /// </summary>
    private async Task CheckUniquenessAsync(DateTime dateFrom, TimeSpan timeFrom, string location)
    {
        var exists = await _tripRepository.AnyAsync(x =>
            x.DateFrom.Date == dateFrom.Date &&
            x.TimeFrom == timeFrom &&
            x.Location == location);

        if (exists)
        {
            throw new BusinessException("Trip:DuplicateExecutionDetails")
                .WithData("DateFrom", dateFrom.ToString("yyyy-MM-dd"))
                .WithData("TimeFrom", timeFrom.ToString(@"hh\:mm"))
                .WithData("Location", location);
        }
    }

    /// <summary>
    /// Validates external trip mandatory fields.
    /// Business Rule: External trips must have provider and embassy requirements.
    /// Clean Code: Focused method, clear error messages.
    /// </summary>
    private void ValidateExternalTripFields(
        TripType type,
        string? tripProvider,
        string? embassyRequirements)
    {
        if (type == TripType.External)
        {
            if (string.IsNullOrWhiteSpace(tripProvider))
            {
                throw new BusinessException("Trip:ProviderRequiredForExternalTrips")
                    .WithData("TripType", type);
            }

            if (string.IsNullOrWhiteSpace(embassyRequirements))
            {
                throw new BusinessException("Trip:EmbassyRequirementsRequiredForExternalTrips")
                    .WithData("TripType", type);
            }
        }
    }

    /// <summary>
    /// Validates that age ranges don't overlap for the same room type.
    /// Business Rule: Within each room type, age ranges must not overlap.
    /// Clean Code: Nested loop for overlap detection, clear logic.
    /// </summary>
    private void ValidateAgeRangeOverlaps(List<CreateTripPricingTierInput> tiers)
    {
        // Group by RoomType for validation
        var tiersByRoom = tiers.GroupBy(t => t.RoomTypeId);

        foreach (var group in tiersByRoom)
        {
            var roomTiers = group.ToList();

            for (int i = 0; i < roomTiers.Count; i++)
            {
                for (int j = i + 1; j < roomTiers.Count; j++)
                {
                    if (RangesOverlap(
                        roomTiers[i].ChildAgeFrom, roomTiers[i].ChildAgeTo,
                        roomTiers[j].ChildAgeFrom, roomTiers[j].ChildAgeTo))
                    {
                        throw new BusinessException("Trip:AgeRangeOverlap")
                            .WithData("RoomTypeId", group.Key)
                            .WithData("Range1From", roomTiers[i].ChildAgeFrom)
                            .WithData("Range1To", roomTiers[i].ChildAgeTo)
                            .WithData("Range2From", roomTiers[j].ChildAgeFrom)
                            .WithData("Range2To", roomTiers[j].ChildAgeTo);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Helper method to check if two age ranges overlap.
    /// Clean Code: Extracted logic for clarity.
    /// </summary>
    private bool RangesOverlap(int from1, int to1, int from2, int to2)
    {
        return from1 <= to2 && from2 <= to1;
    }

    // ============ SUBSCRIPTION METHODS ============

    /// <summary>
    /// Creates a new trip subscription with complete validation.
    /// SRP: Orchestrates validation, cost calculation, and subscription creation.
    /// Business Rules: Deadline check, overlap detection, capacity validation, cost calculation.
    /// </summary>
    public async Task<TripSubscription> SubscribeAsync(
        Guid alumniId,
        Guid tripId,
        Guid roomTypeId,
        List<CreateAttendeeInput> attendees)
    {
        // 1. Load Trip & Validate
        var trip = await _tripRepository.GetAsync(tripId, includeDetails: true);
        ValidateSubscriptionDeadline(trip);

        // 2. Check Time Overlap (cannot be in two trips at same time)
        await CheckTimeOverlapAsync(alumniId, trip.DateFrom, trip.DateTo);

        // 3. Check Capacity (member + attendees)
        var totalAttendees = attendees.Count + 1; // +1 for member
        await CheckCapacityAsync(trip, totalAttendees);

        // 4. Calculate Total Cost
        var totalCost = await CalculateTotalCostAsync(trip, roomTypeId, attendees);

        // 5. Create Subscription
        var subscription = new TripSubscription(
            GuidGenerator.Create(),
            tripId,
            alumniId,
            roomTypeId,
            totalCost,
            trip.AdminFees
        );

        // 6. Add Member as first attendee
        var memberTier = trip.PricingTiers.First(t => t.RoomTypeId == roomTypeId);
        var memberAttendee = new TripSubscriptionAttendee(
            GuidGenerator.Create(),
            subscription.Id,
            "Member", // Name will be populated from AlumniProfile in AppService
            AttendeeType.Member,
            memberTier.MemberPrice,
            true // Member always counts
        );
        subscription.AddAttendee(memberAttendee);

        // 7. Add Companions and Children
        foreach (var attendeeInput in attendees)
        {
            var priceSnapshot = GetAttendeePriceSnapshot(trip, roomTypeId, attendeeInput);
            var countsToCapacity = GetCountsToCapacity(trip, roomTypeId, attendeeInput);

            var attendee = new TripSubscriptionAttendee(
                GuidGenerator.Create(),
                subscription.Id,
                attendeeInput.Name,
                attendeeInput.Type,
                priceSnapshot,
                countsToCapacity,
                attendeeInput.Age,
                attendeeInput.Gender,
                attendeeInput.Relation
            );

            subscription.AddAttendee(attendee);
        }

        return subscription;
    }

    /// <summary>
    /// Cancels a subscription and calculates refund.
    /// Business Rule: Can only cancel before LastCancellationDate.
    /// Refund = TotalPaid - AdminFees (admin fees never refunded).
    /// </summary>
    public async Task<decimal> CancelSubscriptionAsync(Guid subscriptionId)
    {
        // 1. Load subscription and trip
        var subscription = await _subscriptionRepository.GetAsync(subscriptionId);
        var trip = await _tripRepository.GetAsync(subscription.TripId);

        // 2. Validate cancellation deadline
        ValidateCancellationDeadline(trip);

        // 3. Cancel subscription
        subscription.Cancel();

        // 4. Calculate refund amount
        var refundAmount = subscription.CalculateRefundAmount();

        return refundAmount;
    }

    // ============ VALIDATION METHODS ============

    /// <summary>
    /// Validates that current date is before subscription deadline.
    /// Business Rule: Cannot subscribe after LastSubscriptionDate.
    /// </summary>
    private void ValidateSubscriptionDeadline(AlumniTrip trip)
    {
        if (DateTime.Now > trip.LastSubscriptionDate)
        {
            throw new BusinessException("Trip:SubscriptionDeadlinePassed")
                .WithData("LastSubscriptionDate", trip.LastSubscriptionDate)
                .WithData("CurrentDate", DateTime.Now);
        }
    }

    /// <summary>
    /// Validates that current date is before cancellation deadline.
    /// Business Rule: Can only cancel before LastCancellationDate.
    /// </summary>
    private void ValidateCancellationDeadline(AlumniTrip trip)
    {
        if (DateTime.Now > trip.LastCancellationDate)
        {
            throw new BusinessException("Trip:CancellationDeadlinePassed")
                .WithData("LastCancellationDate", trip.LastCancellationDate)
                .WithData("CurrentDate", DateTime.Now);
        }
    }

    /// <summary>
    /// Checks for time overlap with existing active subscriptions.
    /// Business Rule: User cannot be in two trips at the same time.
    /// Clean Code: Single responsibility - overlap detection.
    /// </summary>
    private async Task CheckTimeOverlapAsync(Guid alumniId, DateTime newFrom, DateTime newTo)
    {
        var queryable = await _subscriptionRepository.GetQueryableAsync();
        var tripQueryable = await _tripRepository.GetQueryableAsync();

        var overlapping = await AsyncExecuter.AnyAsync(
            from sub in queryable
            join trip in tripQueryable on sub.TripId equals trip.Id
            where sub.AlumniId == alumniId &&
                  sub.Status == TripSubscriptionStatus.Active &&
                  trip.DateFrom < newTo &&
                  newFrom < trip.DateTo
            select sub
        );

        if (overlapping)
        {
            throw new BusinessException("Trip:TimeOverlapDetected")
                .WithData("AlumniId", alumniId)
                .WithData("NewFrom", newFrom)
                .WithData("NewTo", newTo);
        }
    }

    /// <summary>
    /// Checks if remaining capacity can accommodate the request.
    /// Business Rule: Only count attendees where CountsTowardsCapacity = true.
    /// Clean Code: Focused capacity validation.
    /// </summary>
    private async Task CheckCapacityAsync(AlumniTrip trip, int requestedSeats)
    {
        if (!trip.HasCapacityLimit) return;

        var queryable = await _subscriptionRepository.GetQueryableAsync();

        // Count current subscriptions
        var currentCount = await AsyncExecuter.SumAsync(
            queryable
                .Where(s => s.TripId == trip.Id && s.Status == TripSubscriptionStatus.Active)
                .SelectMany(s => s.Attendees)
                .Where(a => a.CountsTowardsCapacity),
            a => 1
        );

        if (currentCount + requestedSeats > trip.CapacityLimit!.Value)
        {
            throw new BusinessException("Trip:CapacityExceeded")
                .WithData("CurrentCount", currentCount)
                .WithData("RequestedSeats", requestedSeats)
                .WithData("Capacity", trip.CapacityLimit);
        }
    }

    /// <summary>
    /// Calculates total cost for the subscription.
    /// Formula: MemberPrice + (CompanionCount × CompanionPrice) + (ChildCount × ChildPrice) + AdminFees
    /// Clean Code: Separated cost calculation logic.
    /// </summary>
    private Task<decimal> CalculateTotalCostAsync(
        AlumniTrip trip,
        Guid roomTypeId,
        List<CreateAttendeeInput> attendees)
    {
        var tiers = trip.PricingTiers.Where(t => t.RoomTypeId == roomTypeId).ToList();

        if (!tiers.Any())
        {
            throw new BusinessException("Trip:RoomTypeNotFound")
                .WithData("RoomTypeId", roomTypeId);
        }

        decimal total = trip.AdminFees;

        // Member price (always first tier)
        total += tiers.First().MemberPrice;

        // Companions
        var companions = attendees.Where(a => a.Type == AttendeeType.Companion);
        total += companions.Count() * tiers.First().CompanionPrice;

        // Children (by age range)
        var children = attendees.Where(a => a.Type == AttendeeType.Child);
        foreach (var child in children)
        {
            var tier = tiers.FirstOrDefault(t =>
                child.Age!.Value >= t.ChildAgeFrom && child.Age.Value <= t.ChildAgeTo);

            if (tier == null)
            {
                throw new BusinessException("Trip:NoMatchingChildPricingTier")
                    .WithData("ChildAge", child.Age)
                    .WithData("ChildName", child.Name);
            }

            total += tier.ChildPrice;
        }

        return Task.FromResult(total);
    }

    /// <summary>
    /// Gets the price snapshot for an attendee.
    /// Business Rule: Record price at booking time for audit trail.
    /// </summary>
    private decimal GetAttendeePriceSnapshot(AlumniTrip trip, Guid roomTypeId, CreateAttendeeInput attendee)
    {
        var tiers = trip.PricingTiers.Where(t => t.RoomTypeId == roomTypeId).ToList();

        return attendee.Type switch
        {
            AttendeeType.Companion => tiers.First().CompanionPrice,
            AttendeeType.Child => tiers.First(t =>
                attendee.Age!.Value >= t.ChildAgeFrom &&
                attendee.Age.Value <= t.ChildAgeTo).ChildPrice,
            _ => 0
        };
    }

    /// <summary>
    /// Determines if attendee counts towards capacity.
    /// Business Rule: Derived from pricing tier configuration.
    /// </summary>
    private bool GetCountsToCapacity(AlumniTrip trip, Guid roomTypeId, CreateAttendeeInput attendee)
    {
        if (attendee.Type == AttendeeType.Companion) return true;

        if (attendee.Type == AttendeeType.Child)
        {
            var tiers = trip.PricingTiers.Where(t => t.RoomTypeId == roomTypeId);
            var tier = tiers.FirstOrDefault(t =>
                attendee.Age!.Value >= t.ChildAgeFrom &&
                attendee.Age.Value <= t.ChildAgeTo);

            return tier?.CountsTowardsRoomCapacity ?? true;
        }

        return false;
    }
}

/// <summary>
/// Input model for creating pricing tiers.
/// Clean Code: Separate input models from domain entities.
/// </summary>
public class CreateTripPricingTierInput
{
    public Guid RoomTypeId { get; set; }
    public decimal MemberPrice { get; set; }
    public decimal CompanionPrice { get; set; }
    public decimal ChildPrice { get; set; }
    public int ChildAgeFrom { get; set; }
    public int ChildAgeTo { get; set; }
    public bool CountsTowardsRoomCapacity { get; set; }
}

/// <summary>
/// Input model for creating trip requirements.
/// </summary>
public class CreateTripRequirementInput
{
    public Guid DocumentId { get; set; }
    public TripTargetAudience TargetAudience { get; set; }
    public DocumentSubmissionType SubmissionType { get; set; }
}

/// <summary>
/// Input model for creating trip attendees (companions/children).
/// Clean Code: Separate input models from domain entities.
/// </summary>
public class CreateAttendeeInput
{
    public string Name { get; set; } = null!;
    public AttendeeType Type { get; set; }
    public int? Age { get; set; }
    public Gender? Gender { get; set; }
    public CompanionRelation? Relation { get; set; }
}
