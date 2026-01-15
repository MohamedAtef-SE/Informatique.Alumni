using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Informatique.Alumni.Payment;

namespace Informatique.Alumni.Syndicates;

public class SyndicateSubscription : FullAuditedAggregateRoot<Guid>
{
    public Guid AlumniId { get; set; }
    public Guid SyndicateId { get; set; }
    public SyndicateStatus Status { get; set; }
    public decimal FeeAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    
    // Payment Tracking
    public decimal PaidByWallet { get; set; }
    public decimal PaidByGateway { get; set; }
    public string? GatewayToken { get; set; }
    public PaymentGatewayType? PaymentMethod { get; set; }

    public string? AdminNotes { get; set; }
    public ICollection<SyndicateDocument> Documents { get; set; }

    private SyndicateSubscription()
    {
        Documents = new List<SyndicateDocument>();
    }

    public SyndicateSubscription(Guid id, Guid alumniId, Guid syndicateId)
        : base(id)
    {
        AlumniId = alumniId;
        SyndicateId = syndicateId;
        Status = SyndicateStatus.Pending;
        Documents = new List<SyndicateDocument>();
    }

    public void SetReviewing()
    {
        if (Status == SyndicateStatus.Pending)
        {
            Status = SyndicateStatus.Reviewing;
        }
    }

    public void SendToSyndicate()
    {
        if (Status == SyndicateStatus.Reviewing)
        {
            Status = SyndicateStatus.SentToSyndicate;
        }
    }

    public void Complete()
    {
        if (Status == SyndicateStatus.SentToSyndicate)
        {
            Status = SyndicateStatus.CardReady;
        }
    }

    public void Reject(string reason)
    {
        Status = SyndicateStatus.Rejected;
        AdminNotes = reason;
    }

    public void AddDocument(Guid docId, string requirementName, string blobName)
    {
        Documents.Add(new SyndicateDocument(docId, Id, requirementName, blobName));
    }
}
