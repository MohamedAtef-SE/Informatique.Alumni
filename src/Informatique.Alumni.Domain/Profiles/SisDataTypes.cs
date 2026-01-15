using System;
using System.Collections.Generic;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Domain model for Legacy SIS Transcript Data.
/// Used by Domain Services (AlumniManager) to process academic history.
/// </summary>
public class SisQualification
{
    public string QualificationId { get; set; } = string.Empty;
    public string DegreeName { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public string College { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public decimal CumulativeGPA { get; set; }
    public List<SisSemester> Semesters { get; set; } = new();
}

public class SisSemester
{
    public string SemesterCode { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int SemesterNumber { get; set; }
    public decimal SemesterGPA { get; set; }
    public int TotalCredits { get; set; }
    public List<SisCourse> Courses { get; set; } = new();
}

public class SisCourse
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string Grade { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public string InstructorName { get; set; } = string.Empty;
}

// Expected Graduates Types
public class SisExpectedGraduate
{
    public string StudentId { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public decimal GPA { get; set; }
    public int CreditHoursPassed { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime BirthDate { get; set; }
    public string NationalId { get; set; } = string.Empty;
}

public class SisExpectedGraduateFilter
{
    public Guid? BranchId { get; set; }
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    public Guid? MinorId { get; set; }
    public decimal? GpaFrom { get; set; }
    public decimal? GpaTo { get; set; }
    public int? PassedHoursFrom { get; set; }
    public int? PassedHoursTo { get; set; }
    public string? Gender { get; set; }
    public string? Nationality { get; set; }
    public string? IdentityType { get; set; }
    public string? IdentityNumber { get; set; }
    public string? StudentId { get; set; }
    public string? StudentName { get; set; }
    
    // Pagination
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; }
}

public class SisPagedResult<T>
{
    public long TotalCount { get; set; }
    public List<T> Items { get; set; } = new();
    
    public SisPagedResult(long total, List<T> items)
    {
        TotalCount = total;
        Items = items;
    }
}
