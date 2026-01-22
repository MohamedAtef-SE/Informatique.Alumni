using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Guidance;

public class GuidanceSessionRuleDto : FullAuditedEntityDto<Guid>
{
    public Guid AdvisorId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUpdateGuidanceSessionRuleDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

public class AdvisingRequestDto : FullAuditedEntityDto<Guid>
{
    public Guid AlumniId { get; set; }
    public Guid AdvisorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AdvisingRequestStatus Status { get; set; }
}

public class CreateAdvisingRequestDto
{
    public Guid AdvisorId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class BookSessionDto
{
    public Guid AdvisorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AdvisingRequestFilterDto : PagedResultRequestDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public AdvisingRequestStatus? Status { get; set; }
    public Guid? BranchId { get; set; }

    public AdvisingRequestFilterDto()
    {
        var now = DateTime.UtcNow;
        FromDate = new DateTime(now.Year, 1, 1);
        ToDate = new DateTime(now.Year, 12, 31);
        Status = null; // "All" by default
    }
}

public class UpdateAdvisingStatusDto
{
    public AdvisingRequestStatus Status { get; set; }
    public string? Notes { get; set; }
}
