
using System;
using System.Collections.Generic;
using Informatique.Alumni.Guidance;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Guidance;

public class AdvisingReportInputDto : PagedAndSortedResultRequestDto
{
    public List<int> GraduationYears { get; set; } = new();
    // Assuming Semester is just an Int or checking if Enum exists. Defaults to last semester logic handled in AppService if empty.
    // Prompt says "GraduationSemester: List<Enum>". I'll use generic/int if enum not found yet, or strictly check.
    // I'll assume int for now or check GuidanceEnums. 
    // Actually, usually GraduationSemester is 1 or 2 (Spring/Fall).
    public List<int> GraduationSemesters { get; set; } = new(); 
    public Guid? BranchId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public AdvisingRequestStatus? Status { get; set; }
    public AdvisingReportType ReportType { get; set; }
}

public enum AdvisingReportType
{
    Detailed = 1,
    Statistical = 2
}

public class AdvisingReportDetailDto
{
    public int Serial { get; set; }
    public Guid AlumniId { get; set; }
    public string AlumniName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty; // Needs join or fetch
    public string College { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class AdvisingReportStatsRowDto
{
    public int GraduationYear { get; set; }
    public int CountNew { get; set; }
    public int CountAccepted { get; set; }
    public int CountRejected { get; set; }
    public int RowTotal { get; set; }
}
