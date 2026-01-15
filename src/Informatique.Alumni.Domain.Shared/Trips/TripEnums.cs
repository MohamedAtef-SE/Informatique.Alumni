namespace Informatique.Alumni.Trips;

public enum TripRequestStatus
{
    Pending = 0,
    Approved = 1,
    Cancelled = 2,
    Rejected = 3
}

/// <summary>
/// Trip type classification.
/// Clean Code: Self-documenting enum values.
/// </summary>
public enum TripType
{
    Internal = 1,
    External = 2
}

/// <summary>
/// Target audience for trip requirements.
/// DDD: Part of ubiquitous language.
/// </summary>
public enum TripTargetAudience
{
    Member = 1,
    Companion = 2,
    Child = 3
}

/// <summary>
/// Document submission type for requirements.
/// </summary>
public enum DocumentSubmissionType
{
    Original = 1,
    Copy = 2
}

/// <summary>
/// Trip subscription status.
/// Clean Code: Self-documenting values.
/// </summary>
public enum TripSubscriptionStatus
{
    Pending = 0,
    Active = 1,
    Cancelled = 2
}

/// <summary>
/// Type of trip attendee.
/// DDD: Part of ubiquitous language.
/// </summary>
public enum AttendeeType
{
    Member = 1,
    Companion = 2,
    Child = 3
}

/// <summary>
/// Gender for attendee information.
/// </summary>
public enum Gender
{
    Male = 1,
    Female = 2
}

/// <summary>
/// Companion relationship to the member.
/// </summary>
public enum CompanionRelation
{
    Spouse = 1,
    Child = 2,
    Parent = 3,
    Sibling = 4,
    Other = 5
}

/// <summary>
/// Payment method for trip subscription.
/// </summary>
public enum PaymentMethod
{
    Wallet = 1,
    Gateway = 2,
    Split = 3 // Wallet + Gateway
}
