using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Payment;

public class CheckoutDto
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Description { get; set; } = string.Empty;
}

public class PaymentTransactionDto : EntityDto<Guid>
{
    public Guid OrderId { get; set; }
    public string GatewayTransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreationTime { get; set; }
}
