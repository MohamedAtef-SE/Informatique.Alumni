using System;
using System.Collections.Generic;

namespace Informatique.Alumni.Communications;

public class MessageReportInputDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    
    // Filters
    public Guid BranchId { get; set; } // Mandatory
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    
    public List<int> GraduationYears { get; set; } = new();
    public List<int> GraduationSemesters { get; set; } = new(); // Using int for Semester to match Entity (1,2,3)

    public MessageTypeFilter MessageType { get; set; } = MessageTypeFilter.All;
    public ReportType ReportType { get; set; } = ReportType.Detailed;
}

public enum MessageTypeFilter
{
    All = 0,
    SMS = 1,
    Email = 2
}

public enum ReportType
{
    Detailed = 0,
    Statistical = 1
}
