using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Payment;

public class PaymentTransaction : CreationAuditedAggregateRoot<Guid>
{
    public Guid OrderId { get; private set; }
    public string GatewayTransactionId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }

    private PaymentTransaction() 
    {
        GatewayTransactionId = null!;
        Currency = null!;
    }

    public PaymentTransaction(
        Guid id, 
        Guid orderId, 
        decimal amount, 
        string currency = "USD", 
        string? gatewayTransactionId = null) 
        : base(id)
    {
        OrderId = orderId;
        Amount = amount;
        Currency = currency;
        GatewayTransactionId = gatewayTransactionId ?? string.Empty;
        Status = PaymentStatus.Pending;
    }

    public void MarkAsCompleted(string gatewayTransactionId)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("Only Pending transactions can be completed.");
        }
        GatewayTransactionId = gatewayTransactionId;
        Status = PaymentStatus.Completed;
    }

    public void MarkAsFailed(string errorMessage)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("Only Pending transactions can be marked as failed.");
        }
        ErrorMessage = errorMessage;
        Status = PaymentStatus.Failed;
    }
    
    // Immutable: Refund creates a NEW transaction, doesn't modify this one.
}
