using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Profiles;

public class ExpectedGraduateFilterDto : PagedAndSortedResultRequestDto
{
    // Mandatory Filter (Enforced by Security)
    public Guid? BranchId { get; set; }

    // Optional Filters
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; } // Can be string depending on SIS, assuming GUID for now or mapped
    public Guid? MinorId { get; set; }

    public decimal? GpaFrom { get; set; }
    public decimal? GpaTo { get; set; }

    public string? Gender { get; set; } // M/F
    public string? Nationality { get; set; }
    public string? IdentityType { get; set; } // NationalID, Passport
    public string? IdentityNumber { get; set; }

    public string? StudentId { get; set; }
    public string? StudentName { get; set; }
}

public class ExpectedGraduateDto : EntityDto<string> // ID is StudentId (string)
{
    // Basic Info
    public string StudentId { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    
    // Academic Info
    public string BranchName { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public decimal GPA { get; set; }
    public int CreditHoursPassed { get; set; }

    // Contact Info
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }

    // Personal Info
    public DateTime BirthDate { get; set; }
    public string NationalId { get; set; } = string.Empty;
}
