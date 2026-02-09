using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;
using Informatique.Alumni.Membership;

namespace Informatique.Alumni.Syndicates;

public class SyndicateDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public decimal Fee { get; set; }
}

public class CreateUpdateSyndicateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public decimal Fee { get; set; }
}

public class SyndicateSubscriptionDto : FullAuditedEntityDto<Guid>
{
    public Guid AlumniId { get; set; }
    public Guid SyndicateId { get; set; }
    public string SyndicateName { get; set; } = string.Empty;
    public SyndicateStatus Status { get; set; }
    public decimal FeeAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; }
    public string? AdminNotes { get; set; }
    public List<SyndicateDocumentDto> Documents { get; set; } = new();
}

public class SyndicateDocumentDto : EntityDto<Guid>
{
    public string RequirementName { get; set; } = string.Empty;
    public string FileBlobName { get; set; } = string.Empty;
}

public class ApplySyndicateDto
{
    public Guid SyndicateId { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; }
}

public class BatchUpdateStatusDto
{
    public List<Guid> SubscriptionIds { get; set; } = new();
    public SyndicateStatus NewStatus { get; set; }
    public string? AdminNotes { get; set; }
}

public class UploadSyndicateDocDto
{
    public string RequirementName { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
}

public class CreateSyndicateRequestDto
{
    public Guid TargetAlumniId { get; set; }
    public Guid SyndicateId { get; set; }
    public decimal FeeAmount { get; set; }
}

public class SyndicateRequestFilterDto : PagedAndSortedResultRequestDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Guid? BranchId { get; set; }
    public SyndicateStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string? Filter { get; set; } // Alumni details search
}
