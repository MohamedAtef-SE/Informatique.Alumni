namespace Informatique.Alumni.Payments;

public class MembershipCheckoutDto
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
