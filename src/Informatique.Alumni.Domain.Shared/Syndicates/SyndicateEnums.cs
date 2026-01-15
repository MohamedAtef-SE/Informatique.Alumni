namespace Informatique.Alumni.Syndicates;

public enum SyndicateStatus
{
    Pending = 0,
    Reviewing = 1,
    SentToSyndicate = 2,
    CardReady = 3,
    Rejected = 4,
    Received = 5 // Added for tracking delivery
}

public enum SyndicateRequestStatus
{
    New = 0,
    InProgress = 1,
    ReadyForPickup = 3,
    Received = 5
}

public enum PaymentStatus
{
    NotPaid = 0,
    Paid = 1,
    Pending = 2
}
