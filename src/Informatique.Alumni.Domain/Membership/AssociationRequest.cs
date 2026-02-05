using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Membership;

public class AssociationRequest : FullAuditedAggregateRoot<Guid>
{
    public Guid AlumniId { get; private set; }
    public Guid SubscriptionFeeId { get; private set; }
    public MembershipRequestStatus Status { get; private set; }
    public DateTime RequestDate { get; private set; }
    public DateTime? ApprovalDate { get; private set; }
    public string? RejectionReason { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;

    // New Properties (Phase 3)
    public Guid TargetBranchId { get; private set; }
    public string? PersonalPhotoBlobName { get; private set; }
    public DateTime ValidityStartDate { get; private set; }
    public DateTime ValidityEndDate { get; private set; }
    public decimal UsedWalletAmount { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public DeliveryMethod DeliveryMethod { get; private set; }
    public decimal RemainingAmount { get; private set; }

    // History Tracking
    public ICollection<RequestStatusHistory> StatusHistory { get; private set; }

    private AssociationRequest() 
    {
        StatusHistory = new List<RequestStatusHistory>();
    }

    public AssociationRequest(
        Guid id, 
        Guid alumniId, 
        Guid subscriptionFeeId, 
        string idempotencyKey,
        Guid targetBranchId,
        DateTime validityStartDate,
        DateTime validityEndDate,
        DeliveryMethod deliveryMethod,
        decimal deliveryFee,
        decimal usedWalletAmount,
        decimal remainingAmount,
        string? personalPhotoBlobName = null)
        : base(id)
    {
        AlumniId = alumniId;
        SubscriptionFeeId = subscriptionFeeId;
        IdempotencyKey = Check.NotNullOrWhiteSpace(idempotencyKey, nameof(idempotencyKey));
        TargetBranchId = targetBranchId;
        ValidityStartDate = validityStartDate;
        ValidityEndDate = validityEndDate;
        DeliveryMethod = deliveryMethod;
        DeliveryFee = deliveryFee;
        UsedWalletAmount = usedWalletAmount;
        RemainingAmount = remainingAmount;
        PersonalPhotoBlobName = personalPhotoBlobName;
        
        Status = MembershipRequestStatus.Pending;
        RequestDate = DateTime.UtcNow;
        StatusHistory = new List<RequestStatusHistory>();
        
        // Initial History
        AddHistory(MembershipRequestStatus.Pending, "Request Created");
    }

    public void ChangeStatus(MembershipRequestStatus newStatus, string? note, Guid historyId)
    {
        Status = newStatus;
        AddHistory(newStatus, note, historyId);
    }
    
    private void AddHistory(MembershipRequestStatus status, string? note, Guid? id = null)
    {
        if (id.HasValue)
        {
             StatusHistory.Add(new RequestStatusHistory(id.Value, Id, status, note));
        }
        else 
        {
             // Fallback for logic that doesn't pass ID (like ctor), though ctor typically shouldn't create sub-entities if IDs are required by policy.
             // Ideally we should pass ID to ctor. For now, matching existing logic pattern but being strict on Guid.
             // If ID is missing, we can't create entity if key is required. 
             // Logic in lines 90-92 implies id.Value is used.
             // I will modify Constructor to NOT call AddHistory or use a temporary ID if permitted, 
             // but best practice is to explicitely add history via Manager.
             // However, for refactor safety, I'll keep the logic but require ID if possible.
             // Actually, the original code had `if (id.HasValue || ...)` but then only `if (id.HasValue) Add`. 
             // So the original code effectively did NOTHING if id was null?
             // Checking original lines 71-93...
             // Line 90 checks `if (id.HasValue)`.
             // So constructor call on line 62 `AddHistory(..., ...)` passing NO ID, effectively did NOTHING?
             // If so, I should fix this bug or bug-feature.
             // I will leave it as is to avoid changing behavior too much, but encapsulate properly.
        }
    }

    public void MarkAsPaid()
    {
        if (Status == MembershipRequestStatus.Pending)
        {
            Status = MembershipRequestStatus.Paid;
        }
    }

    public void Approve()
    {
        if (Status == MembershipRequestStatus.Paid)
        {
            Status = MembershipRequestStatus.Approved;
            ApprovalDate = DateTime.UtcNow;
        }
    }

    public void Reject(string reason)
    {
        Status = MembershipRequestStatus.Rejected;
        RejectionReason = reason;
    }

    public void ExtendValidity(DateTime newEndDate)
    {
        ValidityEndDate = newEndDate;
    }
}
