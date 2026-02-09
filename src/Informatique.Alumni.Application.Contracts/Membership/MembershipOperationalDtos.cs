using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Membership;

public class MembershipRequestFilterDto : PagedAndSortedResultRequestDto
{
    public Guid? BranchId { get; set; }
    public int? GradYear { get; set; }
    public int? GradSemester { get; set; }
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    public DeliveryMethod? DeliveryMethod { get; set; }
    public MembershipRequestStatus? Status { get; set; }
    // IdentityType/Number not in Request, probably need to join with Profile.
    // For now, these are the main filters requested.
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
    public string? Filter { get; set; } // General search (Alumni Name/ID)
}

public class CardPrintDto
{
    public Guid RequestId { get; set; }
    public string AlumniName { get; set; } = string.Empty;
    public string AlumniNationalId { get; set; } = string.Empty;
    public string AlumniPhotoUrl { get; set; } = string.Empty;
    
    // Last Qualification
    public string Degree { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty; // Resolved from CollegeId? Or just Name if not using Master Data
    public string MajorName { get; set; } = string.Empty;
    public int GradYear { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateStatusDto
{
    public MembershipRequestStatus Status { get; set; }
    public string Note { get; set; } = string.Empty;
}
