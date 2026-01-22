using System;
using System.Collections.Generic;

namespace Informatique.Alumni.Communications;

public class SmsReportResultDto
{
    public List<SmsReportDetailDto> Details { get; set; } = new();
    public List<SmsReportStatsRowDto> Stats { get; set; } = new();
}

public class SmsReportDetailDto
{
    public string MessageTitle { get; set; }
    public DateTime MessageDate { get; set; }
    public string MessageType { get; set; } // SMS/Email
    public Guid AlumniId { get; set; }
    public string AlumniName { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; }
}

public class SmsReportStatsRowDto
{
    public int GraduationYear { get; set; }
    public int MobileCount { get; set; } // Specific naming as per request "Mobile (SMS) Count"
    public int EmailCount { get; set; }
    public int TotalCount { get; set; }
}
