using System;
using System.Collections.Generic;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Contact DTOs for managing multiple emails/mobiles/phones with primary flags.
/// </summary>
public class ContactEmailDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class ContactMobileDto
{
    public Guid Id { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class ContactPhoneDto
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Label { get; set; }
}

/// <summary>
/// Update DTO for alumni profile with contact collections.
/// Business Rules:
/// - Read-Only: Name, Alumni ID, National ID, Nationality, Birthdate, Academic Info
/// - Editable: Photo, Emails (with primary), Mobiles (with primary), Phones, Address
/// </summary>
public class EmployeeUpdateAlumniProfileDto
{
    // Editable Personal Info
    public string? Bio { get; set; }
    public string? JobTitle { get; set; }
    public string? Address { get; set; }
    
    // Photo Upload
    public byte[]? ProfilePhoto { get; set; }
    
    // Contact Collections (Editable)
    public List<ContactEmailDto> Emails { get; set; } = new();
    public List<ContactMobileDto> Mobiles { get; set; } = new();
    public List<ContactPhoneDto> Phones { get; set; } = new();
    
    // Directory Visibility
    public bool ShowInDirectory { get; set; }
}

/// <summary>
/// Import request DTO for syncing from SIS.
/// </summary>
public class ImportAlumniFromSisDto
{
    public string StudentId { get; set; } = string.Empty;
    public string OfficialEmail { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
}


/// <summary>
/// Output DTO for Graduate My Profile.
/// Combines Read-Only SIS Data and Editable Profile Data.
/// </summary>
public class AlumniMyProfileDto
{
    // --- Read-Only Data (Source: SIS/Identity) ---
    public string AlumniId { get; set; } = string.Empty; // StudentId or NationalId
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public decimal OpeningBalance { get; set; } // Financial

    // Academic Info (Read-Only List)
    public List<QualificationHistoryDto> AcademicHistory { get; set; } = new();

    // --- Editable Data ---
    public string? PhotoUrl { get; set; }
    public string? Address { get; set; }
    public string? Bio { get; set; }
    public string? JobTitle { get; set; }

    // Contact Collections
    public List<ContactEmailDto> Emails { get; set; } = new();
    public List<ContactMobileDto> Mobiles { get; set; } = new();
    public List<ContactPhoneDto> Phones { get; set; } = new();
}

/// <summary>
/// Input DTO for Updating Graduate Profile.
/// strictly limited to allowed fields.
/// </summary>
public class UpdateMyProfileDto
{
    // Editable Personal Info
    public string? Address { get; set; }
    public byte[]? ProfilePhoto { get; set; } // Optional upload
    public string? Bio { get; set; } // Optional
    public string? JobTitle { get; set; } // Optional

    // Contact Collections (Full Replacement Logic or Delta)
    public List<ContactEmailDto> Emails { get; set; } = new();
    public List<ContactMobileDto> Mobiles { get; set; } = new();
    public List<ContactPhoneDto> Phones { get; set; } = new();
}
