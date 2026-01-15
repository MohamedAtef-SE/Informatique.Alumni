using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Informatique.Alumni.Certificates;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Input DTO for Alumni Basic Data Report with mandatory filters and defaults.
/// </summary>
public class AlumniReportInputDto : PagedAndSortedResultRequestDto
{
    // Mandatory with defaults
    public List<int>? GraduationYears { get; set; } // Default to last academic year
    public List<int>? GraduationSemesters { get; set; } // Default to last semester
    public DegreeType DegreeType { get; set; } // Mandatory
    public Guid BranchId { get; set; } // Mandatory, enforced by user permissions
    
    // Optional filters
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    public Guid? NationalityId { get; set; }
    
    // Report configuration
    public AlumniReportType ReportType { get; set; } = AlumniReportType.Detailed;
}

/// <summary>
/// Report type enum for alumni reporting
/// </summary>
public enum AlumniReportType
{
    Detailed = 1,    // List view
    Statistical = 2  // Pivot/Cross-tab view
}

/// <summary>
/// Detailed report output DTO (Flat list structure)
/// </summary>
public class AlumniReportDetailDto
{
    public int SerialNo { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AlumniId { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public int GraduationSemester { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public DateTime? CardExpiryDate { get; set; } // Latest active membership card end date
}

/// <summary>
/// Statistical report pivot row DTO (Cross-tab structure)
/// Rows = Year/Semester, Columns = Colleges (dynamic)
/// </summary>
public class AlumniReportStatsRowDto
{
    public int GraduationYear { get; set; }
    public int GraduationSemester { get; set; }
    
    /// <summary>
    /// Dynamic columns: Key = CollegeName, Value = Count of alumni
    /// Example: { "Engineering": 150, "Management": 80, "Medicine": 120 }
    /// </summary>
    public Dictionary<string, int> CollegeCounts { get; set; } = new();
    
    /// <summary>
    /// Row total: Sum of all colleges for this Year/Semester
    /// </summary>
    public int RowTotal { get; set; }
}

/// <summary>
/// Wrapper for statistical report with grand totals
/// </summary>
public class AlumniStatisticalReportDto
{
    public List<AlumniReportStatsRowDto> Rows { get; set; } = new();
    
    /// <summary>
    /// Grand totals per college across all years/semesters
    /// Key = CollegeName, Value = Total count
    /// </summary>
    public Dictionary<string, int> GrandTotalByCollege { get; set; } = new();
    
    /// <summary>
    /// Grand total of all alumni in the report
    /// </summary>
    public int GrandTotal { get; set; }
}

/// <summary>
/// Input DTO for Expected Graduates Report
/// </summary>
public class ExpectedGraduatesReportInputDto : IValidatableObject
{
    // Mandatory Filters
    public Guid BranchId { get; set; }
    public Guid CollegeId { get; set; }

    // Optional Filters
    public Guid? MajorId { get; set; }
    
    public decimal? GpaFrom { get; set; }
    public decimal? GpaTo { get; set; }
    
    public int? PassedHoursFrom { get; set; }
    public int? PassedHoursTo { get; set; }
    
    public string? Sorting { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (GpaFrom.HasValue && GpaTo.HasValue && GpaTo < GpaFrom)
        {
            yield return new ValidationResult(
                "GpaTo must be greater than or equal to GpaFrom.",
                new[] { nameof(GpaFrom), nameof(GpaTo) }
            );
        }

        if (PassedHoursFrom.HasValue && PassedHoursTo.HasValue && PassedHoursTo < PassedHoursFrom)
        {
            yield return new ValidationResult(
                "PassedHoursTo must be greater than or equal to PassedHoursFrom.",
                new[] { nameof(PassedHoursFrom), nameof(PassedHoursTo) }
            );
        }
    }
}

/// <summary>
/// Output DTO for Expected Graduates Report
/// </summary>
public class ExpectedGraduatesReportOutputDto
{
    public int SerialNo { get; set; }
    public string AlumniId { get; set; } = string.Empty;
    public string AlumniName { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public int PassedHours { get; set; }
    public decimal GPA { get; set; }
}
