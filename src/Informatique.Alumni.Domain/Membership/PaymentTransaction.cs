using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Membership;

public class PaymentTransaction : Volo.Abp.Domain.Entities.AggregateRoot<Guid>
{
    public Guid RequestId { get; private set; }
    public decimal Amount { get; private set; }
    public string ExternalTransactionId { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public PaymentStatus Status { get; private set; }

    private PaymentTransaction() 
    {
        ExternalTransactionId = string.Empty;
    }

    public PaymentTransaction(Guid id, Guid requestId, decimal amount, string externalId)
        : base(id)
    {
        RequestId = requestId;
        Amount = amount;
        ExternalTransactionId = externalId;
        PaymentDate = DateTime.UtcNow;
        Status = PaymentStatus.Completed;
    }
}
