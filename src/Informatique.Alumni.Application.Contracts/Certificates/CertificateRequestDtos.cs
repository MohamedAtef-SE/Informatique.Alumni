using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Certificates;

public class CertificateRequestDto : FullAuditedEntityDto<Guid>
{
    public Guid AlumniId { get; set; }
    public List<CertificateRequestItemDto> Items { get; set; } = new();
    public CertificateRequestStatus Status { get; set; }
    
    // Financial properties
    public decimal UsedWalletAmount { get; set; }
    public decimal PaidGatewayAmount { get; set; }
    public decimal TotalItemFees { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal TotalFees { get; set; }
    public decimal RemainingAmount { get; set; }
    
    // Delivery properties
    public DeliveryMethod DeliveryMethod { get; set; }
    public Guid? TargetBranchId { get; set; }
    public string? TargetBranchName { get; set; }
    public string? DeliveryAddress { get; set; }
    
    // Notes
    public string? AdminNotes { get; set; }
    public string? UserNotes { get; set; }
}

public class CertificateRequestItemDto : EntityDto<Guid>
{
    public Guid CertificateRequestId { get; set; }
    public Guid CertificateDefinitionId { get; set; }
    public string CertificateDefinitionName { get; set; } = string.Empty;
    public Guid? QualificationId { get; set; }
    public string? QualificationName { get; set; }
    public CertificateLanguage Language { get; set; }
    public decimal Fee { get; set; }
    
    // Anti-fraud fields
    public string? VerificationHash { get; set; }
    public DateTime? GenerationDate { get; set; }
    public string? QrCodeContent { get; set; }
}

public class CreateCertificateRequestDto
{
    public List<CreateCertificateItemDto> Items { get; set; } = new();
    public DeliveryMethod DeliveryMethod { get; set; }
    public Guid? TargetBranchId { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? UserNotes { get; set; }
}

public class CreateCertificateItemDto
{
    public Guid CertificateDefinitionId { get; set; }
    public Guid? QualificationId { get; set; }
    public CertificateLanguage Language { get; set; }
}

public class ProcessCertificateRequestDto
{
    public CertificateRequestStatus Status { get; set; }
    public string? AdminNotes { get; set; }
}

public class RecordPaymentDto
{
    public decimal Amount { get; set; }
    public string? PaymentReference { get; set; }
}

// Employee Follow-Up DTOs

/// <summary>
/// Advanced filtering for employee certificate request search.
/// Business Rule: Branch security enforced in AppService.
/// </summary>
public class CertificateRequestFilterDto : PagedAndSortedResultRequestDto
{
    public Guid? BranchId { get; set; } // Enforced by security logic
    public int? GraduationYear { get; set; }
    public int? GraduationSemester { get; set; }
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    public DeliveryMethod? DeliveryMethod { get; set; } // Office vs Home filter
    public CertificateRequestStatus? Status { get; set; }
    public Guid? AlumniId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

/// <summary>
/// DTO for updating certificate request status with audit note.
/// </summary>
public class UpdateCertificateStatusDto
{
    public CertificateRequestStatus NewStatus { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// DTO for displaying status change audit history.
/// </summary>
public class CertificateRequestHistoryDto : EntityDto<Guid>
{
    public Guid CertificateRequestId { get; set; }
    public CertificateRequestStatus OldStatus { get; set; }
    public CertificateRequestStatus NewStatus { get; set; }
    public string? Note { get; set; }
    public Guid ChangedByUserId { get; set; }
    public string? ChangedByUserName { get; set; }
    public DateTime ChangedAt { get; set; }
}
