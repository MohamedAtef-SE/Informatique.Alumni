namespace Informatique.Alumni.Payment;

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3
}

public enum PaymentGatewayType
{
    Mock = 0,
    Stripe = 1,
    PayPal = 2
}
