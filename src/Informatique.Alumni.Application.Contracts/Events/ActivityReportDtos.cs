using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Events;

public class ActivityReportInputDto
{
    public Guid BranchId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Guid? ActivityTypeId { get; set; }
    public ReportType ReportType { get; set; } = ReportType.Detailed;
}

public class ActivityReportDetailDto
{
    public int SerialNo { get; set; }
    public string ActivityTypeName { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string DateRange { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class ActivityReportStatsDto
{
    public string ActivityTypeName { get; set; } = string.Empty;
    public int Count { get; set; }
}
