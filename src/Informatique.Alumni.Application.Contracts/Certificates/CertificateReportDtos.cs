using System;
using System.Collections.Generic;

namespace Informatique.Alumni.Certificates;

/// <summary>
/// Input DTO for certificate report generation with mandatory filters and defaults.
/// Business Rules: Respects user data permissions, mandatory fields, optional filters.
/// </summary>
public class CertificateReportInputDto
{
    // Mandatory with defaults
    public List<int>? GraduationYears { get; set; } // Default to current/last academic year
    public List<int>? GraduationSemesters { get; set; } // Default to last semester
    public DegreeType DegreeType { get; set; } // Mandatory
    public Guid BranchId { get; set; } // Mandatory, respects user permissions
    
    // Optional filters
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    public Guid? NationalityId { get; set; }
    public Guid? CertificateTypeId { get; set; } // CertificateDefinitionId
    
    // Report configuration
    public CertificateReportType ReportType { get; set; } = CertificateReportType.Detailed;
    public string? SortBy { get; set; } // "StudentName" or "StudentId"
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// Detailed report output DTO (List view with all student details)
/// </summary>
public class CertificateReportDetailDto
{
    public int SerialNo { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public int GraduationSemester { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public string CertificateName { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Wallet/Gateway
    public DeliveryMethod DeliveryMethod { get; set; }
    public CertificateRequestStatus DeliveryStatus { get; set; }
    public string BranchName { get; set; } = string.Empty; // For grouping
}

/// <summary>
/// Statistical report output DTO (Aggregation view grouped by certificate type)
/// </summary>
public class CertificateReportStatsDto
{
    public string CertificateTypeName { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int InProgressCount { get; set; } // Processing or ReadyForPickup or OutForDelivery
    public int DeliveredCount { get; set; }
    public int NotDeliveredCount { get; set; }
}

/// <summary>
/// Wrapper for statistical report with grand totals
/// </summary>
public class CertificateStatisticalReportDto
{
    public List<CertificateReportStatsDto> Items { get; set; } = new();
    public CertificateReportStatsDto GrandTotal { get; set; } = new();
}
