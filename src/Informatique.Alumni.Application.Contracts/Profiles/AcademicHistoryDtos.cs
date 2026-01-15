using System.Collections.Generic;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Nested DTO hierarchy for Academic History (Read-Only from Legacy SIS)
/// Structure: Qualification → Semester → Courses
/// </summary>
public class AcademicHistoryDto
{
    public List<QualificationHistoryDto> Qualifications { get; set; } = new();
}

/// <summary>
/// Degree-level academic record (e.g., BSc Computer Science, MSc Software Engineering)
/// </summary>
public class QualificationHistoryDto
{
    public string QualificationId { get; set; } = string.Empty;
    public string DegreeName { get; set; } = string.Empty; // e.g., "Bachelor of Science"
    public string Major { get; set; } = string.Empty; // e.g., "Computer Science"
    public string College { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public decimal CumulativeGPA { get; set; }
    
    /// <summary>
    /// Semesters within this qualification
    /// </summary>
    public List<SemesterRecordDto> Semesters { get; set; } = new();
}

/// <summary>
/// Semester/Term-level record (e.g., Fall 2023, Spring 2024)
/// </summary>
public class SemesterRecordDto
{
    public string SemesterCode { get; set; } = string.Empty; // e.g., "2023-1"
    public string SemesterName { get; set; } = string.Empty; // e.g., "Fall 2023"
    public int Year { get; set; }
    public int SemesterNumber { get; set; }
    public decimal SemesterGPA { get; set; }
    public int TotalCredits { get; set; }
    
    /// <summary>
    /// Courses taken in this semester
    /// </summary>
    public List<CourseGradeDto> Courses { get; set; } = new();
}

/// <summary>
/// Individual course grade record (Subject-level)
/// </summary>
public class CourseGradeDto
{
    public string CourseCode { get; set; } = string.Empty; // e.g., "CS101"
    public string CourseName { get; set; } = string.Empty; // e.g., "Introduction to Programming"
    public int Credits { get; set; }
    public string Grade { get; set; } = string.Empty; // e.g., "A", "B+", "C"
    public decimal GradePoint { get; set; } // e.g., 4.0, 3.5, 2.0
    public string InstructorName { get; set; } = string.Empty;
}
