using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Guidance;

public class GuidanceSessionRuleDto : FullAuditedEntityDto<Guid>
{
    public Guid BranchId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SessionDurationMinutes { get; set; }
    public List<DayOfWeek> WeekDays { get; set; } = new();
}

public class UpdateGuidanceSessionRuleDto
{
    public Guid BranchId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SessionDurationMinutes { get; set; }
    public List<DayOfWeek> WeekDays { get; set; } = new();
}

public class AdvisingRequestDto : FullAuditedEntityDto<Guid>
{
    public Guid AlumniId { get; set; }
    public Guid AdvisorId { get; set; }
    public string? AdvisorName { get; set; }
    public string? AdvisorJobTitle { get; set; }
    public string? Location { get; set; } = "Online (Google Meet)"; // Defaulting for now as per user complaint
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

public class AdvisorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? PhotoUrl { get; set; }
    public Guid BranchId { get; set; }
}
