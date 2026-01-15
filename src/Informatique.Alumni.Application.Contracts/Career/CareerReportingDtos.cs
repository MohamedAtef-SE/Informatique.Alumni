using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Career;

public class CareerParticipantReportInputDto
{
    public Guid CareerServiceTypeId { get; set; }
    public Guid BranchId { get; set; } // Read-Only from UI, set by Backend based on User claims or logic
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    public Guid? NationalityId { get; set; }
    public List<int> GraduationYear { get; set; } = new List<int>();
    public CareerReportType ReportType { get; set; } = CareerReportType.Detailed;
}

public class CareerParticipantReportDetailDto
{
    public int SerialNo { get; set; }
    public Guid AlumniId { get; set; }
    public string AlumniName { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public string ServiceTypeName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
}

public class CareerParticipantReportStatsDto
{
    public string ServiceTypeName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string DateRange { get; set; } = string.Empty;
    public int AttendanceCount { get; set; }
}

public enum CareerReportType
{
    Detailed = 1,
    Statistical = 2
}
