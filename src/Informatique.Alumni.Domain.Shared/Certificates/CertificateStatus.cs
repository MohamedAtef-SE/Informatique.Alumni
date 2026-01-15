namespace Informatique.Alumni.Certificates;

public enum CertificateRequestStatus
{
    Draft = 1,
    PendingPayment = 2,
    Processing = 3,
    ReadyForPickup = 4,
    OutForDelivery = 5,
    Delivered = 6,
    Rejected = 7
}
