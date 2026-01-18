using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Events;

public class ParticipantReportInputDto
{
    public Guid ActivityTypeId { get; set; }
    public Guid BranchId { get; set; } // Read-Only (Enforced in AppService)
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    // Demographics Filters
    public Guid? CollegeId { get; set; }
    public List<int> GraduationYear { get; set; } = new();
    public Guid? MajorId { get; set; }
    public Guid? NationalityId { get; set; }
    
    public ReportType ReportType { get; set; } = ReportType.Detailed;
}

public enum ReportType
{
    Detailed = 0, // Roster View
    Statistical = 1 // Summary Counts
}

public class ParticipantReportDetailDto
{
    public int SerialNo { get; set; }
    public Guid AlumniId { get; set; }
    public string AlumniName { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public string ActivityTypeName { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string DateRange { get; set; } = string.Empty;
}

public class ParticipantReportStatsDto
{
    public string ActivityTypeName { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string DateRange { get; set; } = string.Empty;
    public int AttendanceCount { get; set; }
}
