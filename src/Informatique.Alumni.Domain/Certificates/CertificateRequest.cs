using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Certificates;

public class CertificateRequest : FullAuditedAggregateRoot<Guid>
{
    public Guid AlumniId { get; private set; }
    public CertificateRequestStatus Status { get; private set; }
    public string? AdminNotes { get; private set; }
    public string? UserNotes { get; private set; }
    
    // Financial fields (Business Rule #3: Wallet Logic)
    public decimal UsedWalletAmount { get; private set; }
    public decimal PaidGatewayAmount { get; private set; }
    public decimal TotalItemFees { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public decimal TotalFees => TotalItemFees + DeliveryFee;
    public decimal RemainingAmount => TotalFees - UsedWalletAmount - PaidGatewayAmount;
    
    // Delivery fields (Business Rule #4: Logistics & Delivery)
    public DeliveryMethod DeliveryMethod { get; private set; }
    public Guid? TargetBranchId { get; private set; }
    public string? DeliveryAddress { get; private set; }
    
    // Multi-item support (Business Rule #2: Multi-Item Request)
    private readonly List<CertificateRequestItem> _items = new();
    public IReadOnlyCollection<CertificateRequestItem> Items => _items.AsReadOnly();
    
    // Status audit history (Employee Follow-Up: Audit Trail)
    private readonly List<CertificateRequestHistory> _history = new();
    public IReadOnlyCollection<CertificateRequestHistory> History => _history.AsReadOnly();

    private CertificateRequest() { }

    public CertificateRequest(
        Guid id,
        Guid alumniId,
        DeliveryMethod deliveryMethod,
        Guid? targetBranchId = null,
        string? deliveryAddress = null,
        string? userNotes = null)
        : base(id)
    {
        AlumniId = Check.NotDefaultOrNull<Guid>(alumniId, nameof(alumniId));
        DeliveryMethod = deliveryMethod;
        TargetBranchId = targetBranchId;
        DeliveryAddress = deliveryAddress;
        
        if (userNotes != null)
        {
            UserNotes = Check.Length(userNotes, nameof(userNotes), CertificateConsts.MaxNotesLength);
        }
        
        Status = CertificateRequestStatus.Draft;
        UsedWalletAmount = 0;
        PaidGatewayAmount = 0;
        DeliveryFee = 0;
        TotalItemFees = 0;
    }

    public void AddItem(CertificateRequestItem item)
    {
        Check.NotNull(item, nameof(item));
        
        if (Status != CertificateRequestStatus.Draft)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.CannotModifyNonDraft)
                .WithData("CurrentStatus", Status);
        }
        
        _items.Add(item);
        RecalculateTotalItemFees();
    }

    public void RemoveItem(Guid itemId)
    {
        if (Status != CertificateRequestStatus.Draft)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.CannotModifyNonDraft)
                .WithData("CurrentStatus", Status);
        }
        
        var item = _items.FirstOrDefault(x => x.Id == itemId);
        if (item == null)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.ItemNotFound)
                .WithData("ItemId", itemId);
        }
        
        _items.Remove(item);
        RecalculateTotalItemFees();
    }

    public void SetDeliveryFee(decimal deliveryFee)
    {
        if (deliveryFee < 0)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.InvalidDeliveryFee)
                .WithData("DeliveryFee", deliveryFee);
        }
        
        DeliveryFee = deliveryFee;
    }

    public void ApplyWalletDeduction(decimal amount)
    {
        if (amount < 0)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.NegativeWalletDeduction)
                .WithData("Amount", amount);
        }
        
        if (amount > TotalFees)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.ExcessiveWalletDeduction)
                .WithData("Amount", amount)
                .WithData("TotalFees", TotalFees);
        }
        
        UsedWalletAmount = amount;
        
        // Update status based on remaining amount
        if (RemainingAmount > 0)
        {
            Status = CertificateRequestStatus.PendingPayment;
        }
        else
        {
            Status = CertificateRequestStatus.Processing;
        }
    }

    public void RecordGatewayPayment(decimal amount)
    {
        if (amount <= 0)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.InvalidPaymentAmount)
                .WithData("Amount", amount);
        }
        
        if (Status != CertificateRequestStatus.PendingPayment)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.InvalidStatusForPayment)
                .WithData("CurrentStatus", Status);
        }
        
        PaidGatewayAmount += amount;
        
        // Check if fully paid
        if (RemainingAmount <= 0)
        {
            Status = CertificateRequestStatus.Processing;
        }
    }

    public void MoveToProcessing()
    {
        if (Status != CertificateRequestStatus.Draft && Status != CertificateRequestStatus.PendingPayment)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.InvalidStatusTransition)
                .WithData("CurrentStatus", Status)
                .WithData("TargetStatus", CertificateRequestStatus.Processing);
        }
        
        // Business Rule: Cannot proceed without full payment
        if (RemainingAmount > 0)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.PaymentRequired)
                .WithData("RemainingAmount", RemainingAmount);
        }
        
        Status = CertificateRequestStatus.Processing;
    }

    public void MarkAsReadyForPickup()
    {
        if (Status != CertificateRequestStatus.Processing)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.InvalidStatusTransition)
                .WithData("CurrentStatus", Status)
                .WithData("TargetStatus", CertificateRequestStatus.ReadyForPickup);
        }
        
        if (DeliveryMethod != DeliveryMethod.BranchPickup)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.InvalidDeliveryMethod)
                .WithData("CurrentDeliveryMethod", DeliveryMethod)
                .WithData("RequiredDeliveryMethod", DeliveryMethod.BranchPickup);
        }
        
        Status = CertificateRequestStatus.ReadyForPickup;
    }

    public void MarkAsOutForDelivery()
    {
        if (Status != CertificateRequestStatus.Processing)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.InvalidStatusTransition)
                .WithData("CurrentStatus", Status)
                .WithData("TargetStatus", CertificateRequestStatus.OutForDelivery);
        }
        
        if (DeliveryMethod != DeliveryMethod.HomeDelivery)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.InvalidDeliveryMethod)
                .WithData("CurrentDeliveryMethod", DeliveryMethod)
                .WithData("RequiredDeliveryMethod", DeliveryMethod.HomeDelivery);
        }
        
        Status = CertificateRequestStatus.OutForDelivery;
    }

    public void Deliver()
    {
        if (Status != CertificateRequestStatus.ReadyForPickup && Status != CertificateRequestStatus.OutForDelivery)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.InvalidStatusTransition)
                .WithData("CurrentStatus", Status)
                .WithData("TargetStatus", CertificateRequestStatus.Delivered);
        }
        
        Status = CertificateRequestStatus.Delivered;
    }

    public void Reject(string reason)
    {
        if (Status == CertificateRequestStatus.Delivered)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.CannotRejectDelivered);
        }
        
        AdminNotes = Check.NotNullOrWhiteSpace(reason, nameof(reason), CertificateConsts.MaxNotesLength);
        Status = CertificateRequestStatus.Rejected;
    }

    public void AddAdminNotes(string notes)
    {
        AdminNotes = Check.NotNullOrWhiteSpace(notes, nameof(notes), CertificateConsts.MaxNotesLength);
    }

    /// <summary>
    /// Employee Follow-Up: Change status with conditional state machine validation.
    /// Business Rule: "Two Paths" - transitions depend on DeliveryMethod.
    /// </summary>
    public CertificateRequestHistory ChangeStatus(
        CertificateRequestStatus newStatus,
        Guid changedByUserId,
        Func<Guid> guidGenerator,
        string? note = null)
    {
        // Validate transition based on delivery method (Two Paths Rule)
        ValidateStatusTransition(newStatus);
        
        var oldStatus = Status;
        Status = newStatus;
        
        // Create audit history entry
        var historyEntry = new CertificateRequestHistory(
            guidGenerator(),
            Id,
            oldStatus,
            newStatus,
            changedByUserId,
            note
        );
        
        _history.Add(historyEntry);
        
        return historyEntry;
    }

    private void ValidateStatusTransition(CertificateRequestStatus newStatus)
    {
        // Business Rule: "Two Paths" - Conditional State Machine based on Delivery Method
        
        // Path A: Office Pickup
        // Processing → ReadyForPickup → Delivered
        if (DeliveryMethod == DeliveryMethod.BranchPickup)
        {
            if (newStatus == CertificateRequestStatus.OutForDelivery)
            {
                throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.InvalidStatusForDeliveryMethod)
                    .WithData("CurrentDeliveryMethod", DeliveryMethod)
                    .WithData("AttemptedStatus", newStatus)
                    .WithData("AllowedStatuses", "Processing, ReadyForPickup, Delivered, Rejected");
            }
        }
        
        // Path B: Home Delivery
        // Processing → OutForDelivery → Delivered
        if (DeliveryMethod == DeliveryMethod.HomeDelivery)
        {
            if (newStatus == CertificateRequestStatus.ReadyForPickup)
            {
                throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.InvalidStatusForDeliveryMethod)
                    .WithData("CurrentDeliveryMethod", DeliveryMethod)
                    .WithData("AttemptedStatus", newStatus)
                    .WithData("AllowedStatuses", "Processing, OutForDelivery, Delivered, Rejected");
            }
        }
        
        // General state machine validation
        if (Status == CertificateRequestStatus.Delivered && newStatus != CertificateRequestStatus.Delivered)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.CannotModifyDelivered);
        }
        
        if (Status == CertificateRequestStatus.Rejected && newStatus != CertificateRequestStatus.Rejected)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.CannotModifyRejected);
        }
    }

    private void RecalculateTotalItemFees()
    {
        TotalItemFees = _items.Sum(x => x.Fee);
    }
}
