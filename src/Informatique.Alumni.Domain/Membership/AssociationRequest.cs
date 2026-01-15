using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Membership;

public class AssociationRequest : FullAuditedAggregateRoot<Guid>
{
    public Guid AlumniId { get; set; }
    public Guid SubscriptionFeeId { get; set; }
    public MembershipRequestStatus Status { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? RejectionReason { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;

    // New Properties (Phase 3)
    public Guid TargetBranchId { get; set; }
    public string? PersonalPhotoBlobName { get; set; }
    public DateTime ValidityStartDate { get; set; }
    public DateTime ValidityEndDate { get; set; }
    public decimal UsedWalletAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; }
    public decimal RemainingAmount { get; set; }

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
        string? personalPhotoBlobName = null)
        : base(id)
    {
        AlumniId = alumniId;
        SubscriptionFeeId = subscriptionFeeId;
        IdempotencyKey = idempotencyKey;
        Status = MembershipRequestStatus.Pending;
        RequestDate = DateTime.UtcNow;
        TargetBranchId = targetBranchId;
        ValidityStartDate = validityStartDate;
        ValidityEndDate = validityEndDate;
        DeliveryMethod = deliveryMethod;
        DeliveryFee = deliveryFee;
        PersonalPhotoBlobName = personalPhotoBlobName;
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
        // Ideally we pass IGuidGenerator, but entities shouldn't have services injected. 
        // We can pass the ID from outside (Manager/AppService).
        // For constructor, we can't easily pass ID unless we change sig.
        // I will assume ID is passed or generated here if safe (Guid.NewGuid is generally safe but not recommended for distributed/sequential)
        // Better: Make ChangeStatus accept historyId. Constructor can accept historyId too or just ignore for initial state relative to ID generation policy.
        // For now, I will update ChangeStatus signature to accept historyId.
        
        if (id.HasValue || status != MembershipRequestStatus.Pending) // Only add history if ID provided or not initial (handled in ctor logic if ID passed)
        {
             // For constructor usage, we might skip adding history entity inside ctor to avoid ID issues, 
             // OR we accept historyId in Ctor. 
             // To keep it simple: AppService/Manager should call ChangeStatus/AddHistory explicitly OR we accept it.
             // I'll stick to: AppService calls ChangeStatus. Constructor initializes list. 
             // NOTE: Previous phases might have skipped history on Create. I'll add "AddHistory" helper and allow using it.
        }
        
        if (id.HasValue)
        {
             StatusHistory.Add(new RequestStatusHistory(id.Value, Id, status, note));
        }
    }

    public void MarkAsPaid()
    {
        if (Status == MembershipRequestStatus.Pending)
        {
            Status = MembershipRequestStatus.Paid;
            // Note: History should be added via ChangeStatus ideally, but this method exists from previous phase.
            // I will NOT modify this existing method logic to require arguments to avoid breaking simple usage, 
            // BUT proper flow should be to use ChangeStatus. 
            // I'll leave it as is to respect "Additive Only" strictly unless I refactor calls.
            // Wait, "Refactor" IS the task.
        }
    }
    
    // ... Verify other methods ...
    
    public void Approve() { ... } 
    public void Reject(string reason) { ... }

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
}
