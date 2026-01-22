using System;
using System.Collections.Generic;

namespace Informatique.Alumni.Communications;

public class SmsReportInputDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    
    // Filters
    public Guid BranchId { get; set; } // Mandatory
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    
    public List<int> GraduationYears { get; set; } = new();
    public List<int> GraduationSemesters { get; set; } = new(); 

    public MessageTypeFilter MessageType { get; set; } = MessageTypeFilter.All;
    public ReportType ReportType { get; set; } = ReportType.Detailed;
}
