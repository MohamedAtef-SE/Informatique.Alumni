namespace Informatique.Alumni.Membership;

public enum MembershipRequestStatus
{
    Pending = 1,
    Paid = 2,
    Approved = 3,
    Approved = 3,
    Rejected = 4,
    InProgress = 5,
    ReadyForPickup = 6,
    OutForDelivery = 7,
    Delivered = 8
}

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,

public enum DeliveryMethod
{
    OfficePickup = 1,
    HomeDelivery = 2
}

public enum AccessPolicy
{
    AllAlumni = 1,
    ActiveMembersOnly = 2
}
