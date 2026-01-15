using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Career;

public class CareerServicesReportInputDto
{
    public Guid BranchId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Guid? CareerServiceTypeId { get; set; }
    public CareerReportType ReportType { get; set; } = CareerReportType.Detailed;
}

public class CareerServicesReportDetailDto
{
    public int SerialNo { get; set; }
    public string ServiceTypeName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string DateRange { get; set; } = null!;
    public string Location { get; set; } = null!;
}

public class CareerServicesReportStatsDto
{
    public string ServiceTypeName { get; set; } = null!;
    public int Count { get; set; }
}
