using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Membership;

public class SubscriptionFeeDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Year { get; set; }
    public DateTime SeasonStartDate { get; set; }
    public DateTime SeasonEndDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSubscriptionFeeDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Year { get; set; }
    public DateTime SeasonStartDate { get; set; }
    public DateTime SeasonEndDate { get; set; }
}

public class AssociationRequestDto : FullAuditedEntityDto<Guid>
{
    public Guid AlumniId { get; set; }
    public Guid SubscriptionFeeId { get; set; }
    public string SubscriptionFeeName { get; set; } = string.Empty;
    public MembershipRequestStatus Status { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? RejectionReason { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
}

public class CreateAssociationRequestDto
{
    public Guid SubscriptionFeeId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    
    // New Fields (Phase 3)
    public Guid TargetBranchId { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; }
    
    // Contact & Profile Updates
    public List<ContactUpdateDto> Emails { get; set; } = new();
    public List<ContactUpdateDto> Mobiles { get; set; } = new();
    public string? Address { get; set; }
    
    // Personal Photo
    public string? PersonalPhotoFileName { get; set; }
    public byte[]? PersonalPhotoBytes { get; set; }
}

public class ContactUpdateDto
{
    public string Value { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class MembershipPaymentDto
{
    public Guid RequestId { get; set; }
    public decimal Amount { get; set; }
    public string ExternalTransactionId { get; set; } = string.Empty;
}
