using System;
using System.Collections.Generic;

namespace Informatique.Alumni.Communications;

public class MessageReportResultDto
{
    public List<MessageReportDetailDto> Details { get; set; } = new();
    public List<MessageReportStatsRowDto> Stats { get; set; } = new();
}

public class MessageReportDetailDto
{
    public string MessageTitle { get; set; }
    public DateTime MessageDate { get; set; }
    public string MessageType { get; set; } // SMS/Email
    public Guid AlumniId { get; set; }
    public string AlumniName { get; set; }
    public Guid BranchId { get; set; } // For Grouping
    public string BranchName { get; set; } // Optional
}

public class MessageReportStatsRowDto
{
    public int GraduationYear { get; set; }
    public int SmsCount { get; set; }
    public int EmailCount { get; set; }
    public int TotalCount { get; set; }
}
