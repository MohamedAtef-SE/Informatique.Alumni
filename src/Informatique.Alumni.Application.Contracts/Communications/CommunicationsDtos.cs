using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Communications;

/// <summary>
/// Filter for selecting alumni for bulk communication.
/// </summary>
public class AlumniCommunicationFilterDto
{
    // Mandatory
    public Guid BranchId { get; set; }
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
