using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Communications;

/// <summary>
/// Filter for selecting alumni for bulk communication.
/// </summary>
public class AlumniCommunicationFilterDto
{
    // Optional Branch
    public Guid? BranchId { get; set; }
    public int? GraduationYear { get; set; }
    public int? GraduationSemester { get; set; }

    // Optional
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    public Guid? MinorId { get; set; }
    
    // Status
    public CommunicationMembershipStatus? MembershipStatus { get; set; } = CommunicationMembershipStatus.All;

    // Academic
    public decimal? GpaFrom { get; set; }
    public decimal? GpaTo { get; set; }
}

public class SendGeneralMessageInputDto
{
    public CommunicationChannel Channel { get; set; } // Email / SMS
    public string Subject { get; set; } = string.Empty; // Required for Email
    public string Body { get; set; } = string.Empty;
    // Attachments: Mocking just URLs or simple strings for now
    public string[]? AttachmentUrls { get; set; }

    public AlumniCommunicationFilterDto Filter { get; set; } = new();
}

public enum CommunicationChannel
{
    Email = 1,
    Sms = 2
}

public enum CommunicationMembershipStatus
{
    All = 0,
    Active = 1,
    Inactive = 2
}

public class GeneralMessageSenderJobArgs
{
    public AlumniCommunicationFilterDto Filter { get; set; } = new();
    public CommunicationChannel Channel { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string[]? AttachmentUrls { get; set; }
    public Guid SenderId { get; set; }
    public Guid? TenantId { get; set; }
}

public class CommunicationLogDto : Volo.Abp.Application.Dtos.EntityDto<Guid>
{
    public Guid SenderId { get; set; }
    public Guid? RecipientId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? AlumniName { get; set; }
    public string? TargetAddress { get; set; }
    public DateTime CreationTime { get; set; }
}

public class GetCommunicationLogInputDto : Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public string? Channel { get; set; }
    public string? Status { get; set; }
    public Guid? RecipientId { get; set; }
}
