using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Search and filter DTO for alumni list with hybrid data support.
/// Business Rules: Mandatory filters (Year, Semester, Branch), Optional filters (College, Major, GPA, etc.)
/// </summary>
public class AlumniSearchFilterDto : PagedAndSortedResultRequestDto
{
    // Mandatory Filters
    public List<int>? GraduationYears { get; set; }
    public List<int>? GraduationSemesters { get; set; }
    public Guid BranchId { get; set; } // Enforced by security
    
    // Optional Filters - Academic
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    public Guid? MinorId { get; set; }
    
    // Optional Filters - GPA Range
    public decimal? GpaMin { get; set; }
    public decimal? GpaMax { get; set; }
    
    // Optional Filters - Demographics
    public string? Gender { get; set; }
    public Guid? NationalityId { get; set; }
    
    // Local Extensions
    public VipFilterOption IsVip { get; set; } = VipFilterOption.All;
}

/// <summary>
/// VIP filter options for alumni search
/// </summary>
public enum VipFilterOption
{
    All = 0,      // Show all alumni
    VipOnly = 1,  // Show only VIP alumni
    NonVipOnly = 2 // Show only non-VIP alumni
}

/// <summary>
/// Alumni list DTO (Flat structure - Latest Degree only).
/// Business Rule: Display only the latest qualification in search results.
/// </summary>
public class AlumniListDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    // Personal Info
    public string Name { get; set; } = string.Empty;
    public string AlumniId { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public string? Nationality { get; set; }
    
    // Latest Qualification Only
    public int GraduationYear { get; set; }
    public int GraduationSemester { get; set; }
    public string College { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public string? Minor { get; set; }
    public decimal GPA { get; set; }
    
    // Contact Info (Primary only)
    public string? PrimaryEmail { get; set; }
    public string? PrimaryMobile { get; set; }
    
    // Local Extensions
    public bool IsVip { get; set; }
    public string? PhotoUrl { get; set; }
}

/// <summary>
/// Detailed alumni profile DTO with all qualifications.
/// Business Rule: Show ALL qualifications in profile view, not just latest.
/// </summary>
public class AlumniProfileDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    // Personal Info (Read-Only)
    public string Name { get; set; } = string.Empty;
    public string AlumniId { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTime? Birthdate { get; set; }
    public string? Gender { get; set; }
    public string? Nationality { get; set; }
    
    // All Qualifications (History)
    public List<AlumniEducationDto> Educations { get; set; } = new();
    
    // Contact Information
    public List<ContactEmailDto> Emails { get; set; } = new();
    public List<ContactMobileDto> Mobiles { get; set; } = new();
    public List<ContactPhoneDto> Phones { get; set; } = new();
    
    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    
    // Professional
    public string? JobTitle { get; set; }
    public string? Bio { get; set; }
    public List<ExperienceDto> Experiences { get; set; } = new();
    
    // Financial (Read-Only)
    public decimal WalletBalance { get; set; }
    public decimal OpeningBalance { get; set; }
    
    // Local Extensions
    public bool IsVip { get; set; }
    public bool ShowInDirectory { get; set; }
    
    // Photo (Editable Exception)
    public string? PhotoUrl { get; set; }
}

/// <summary>
/// Education record DTO for displaying all alumni qualifications
/// </summary>
public class AlumniEducationDto
{
    public Guid Id { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public int GraduationSemester { get; set; }
    public string? College { get; set; }
    public string? Major { get; set; }
    public string? Minor { get; set; }
    public decimal? GPA { get; set; }
}

/// <summary>
/// DTO for updating alumni photo (The only editable field for employees)
/// </summary>
public class UpdateAlumniPhotoDto
{
    public byte[] PhotoData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/jpeg";
}
