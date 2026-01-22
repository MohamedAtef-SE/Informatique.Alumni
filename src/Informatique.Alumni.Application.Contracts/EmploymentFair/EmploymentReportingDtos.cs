using System;
using System.Collections.Generic;

namespace Informatique.Alumni.EmploymentFair;

public class EmploymentStatsDto
{
    // For Audit Stats
    public string GroupKey { get; set; } // Graduation Year or Company
    public int TotalCount { get; set; }
    public Dictionary<string, int> StatusCounts { get; set; } = new();
}

public class SentCvStatsDto
{
    public string CompanyEmail { get; set; }
    public int SentCount { get; set; }
    public DateTime LastSentDate { get; set; }
}

public class CvAuditStatsDto
{
    public int GraduationYear { get; set; }
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int TotalCount { get; set; }
}
