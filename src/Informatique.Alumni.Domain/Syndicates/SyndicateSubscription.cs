using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Informatique.Alumni.Payment;
using Informatique.Alumni.Membership;

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
    
    public DeliveryMethod DeliveryMethod { get; private set; }

    public string? AdminNotes { get; private set; }
    public ICollection<SyndicateDocument> Documents { get; private set; }

    private SyndicateSubscription()
    {
        Documents = new List<SyndicateDocument>();
    }

    public SyndicateSubscription(Guid id, Guid alumniId, Guid syndicateId, decimal feeAmount, DeliveryMethod deliveryMethod = DeliveryMethod.OfficePickup)
        : base(id)
    {
        AlumniId = alumniId;
        SyndicateId = syndicateId;
        FeeAmount = feeAmount;
        Status = SyndicateStatus.Draft; // = New / Draft
        PaymentStatus = PaymentStatus.NotPaid;
        DeliveryMethod = deliveryMethod;
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
    
    public void ConfirmPayment(decimal amount, string gatewayToken, PaymentGatewayType paymentMethod)
    {
        if (PaymentStatus == PaymentStatus.Paid) return;

        PaymentStatus = PaymentStatus.Paid;
        PaidByGateway = amount;
        GatewayToken = gatewayToken;
        PaymentMethod = paymentMethod;
        
        // Auto-transition status if needed
        if (Status == SyndicateStatus.Draft)
        {
             Status = SyndicateStatus.Pending;
        }
    }

    internal void SetPaymentStatus(PaymentStatus status)
    {
        PaymentStatus = status;
    }

    public void AddDocument(Guid docId, string requirementName, string blobName)
    {
        Documents.Add(new SyndicateDocument(docId, Id, requirementName, blobName));
    }
}
