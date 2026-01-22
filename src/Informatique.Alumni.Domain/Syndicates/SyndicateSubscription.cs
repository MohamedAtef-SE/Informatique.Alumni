using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Informatique.Alumni.Payment;

namespace Informatique.Alumni.Syndicates;

public class SyndicateSubscription : FullAuditedAggregateRoot<Guid>
{
    public Guid AlumniId { get; private set; }
    public Guid SyndicateId { get; private set; }
    public SyndicateStatus Status { get; private set; }
    public decimal FeeAmount { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    
    // Split Payment Tracking
    // Split Payment Tracking
    public decimal PaidByWallet { get; private set; }
    public decimal PaidByGateway { get; private set; }
    public string? GatewayToken { get; private set; }
    public PaymentGatewayType? PaymentMethod { get; private set; }

    public string? AdminNotes { get; private set; }
    public ICollection<SyndicateDocument> Documents { get; private set; }

    private SyndicateSubscription()
    {
        Documents = new List<SyndicateDocument>();
    }

    public SyndicateSubscription(Guid id, Guid alumniId, Guid syndicateId, decimal feeAmount)
        : base(id)
    {
        AlumniId = alumniId;
        SyndicateId = syndicateId;
        FeeAmount = feeAmount;
        Status = SyndicateStatus.Pending; // = New
        PaymentStatus = PaymentStatus.NotPaid;
        Documents = new List<SyndicateDocument>();
    }

    public void MarkAsInProgress()
    {
        // New -> InProgress
        if (Status == SyndicateStatus.Pending)
        {
            Status = SyndicateStatus.Reviewing; // Mapped to InProgress
        }
    }

    public void MarkAsReadyForPickup()
    {
        // InProgress -> ReadyForPickup
        if (Status == SyndicateStatus.Reviewing)
        {
            Status = SyndicateStatus.CardReady; // Mapped to ReadyForPickup
        }
    }

    public void MarkAsReceived()
    {
        // ReadyForPickup -> Received
        if (Status == SyndicateStatus.CardReady)
        {
            Status = SyndicateStatus.Received;
        }
    }

    public void Reject(string reason)
    {
        Status = SyndicateStatus.Rejected;
        AdminNotes = reason;
    }
    
    internal void SetPaymentStatus(PaymentStatus status)
    {
        PaymentStatus = status;
    }

    internal void InitializePayment(decimal paidByWallet, decimal paidByGateway, PaymentGatewayType? paymentMethod)
    {
        PaidByWallet = paidByWallet;
        PaidByGateway = paidByGateway;
        PaymentMethod = paymentMethod;
    }

    public void AddDocument(Guid docId, string requirementName, string blobName)
    {
        Documents.Add(new SyndicateDocument(docId, Id, requirementName, blobName));
    }
}
