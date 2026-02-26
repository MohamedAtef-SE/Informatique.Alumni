using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Admin;

// ── Job Admin ──

public class JobAdminDto : EntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Requirements { get; set; }
    public Guid CompanyId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ClosingDate { get; set; }
    public DateTime CreationTime { get; set; }
    public int ApplicationCount { get; set; }
}

public class JobApplicationAdminDto : EntityDto<Guid>
{
    public Guid JobId { get; set; }
    public Guid AlumniId { get; set; }
    public string AlumniName { get; set; } = string.Empty;
    public string CvSnapshotBlobName { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
}

public class JobAdminGetListInput : PagedAndSortedResultRequestDto
{
    public bool? IsActive { get; set; }
    public string? Filter { get; set; }
}

public interface IJobAdminAppService : IApplicationService
{
    Task<PagedResultDto<JobAdminDto>> GetListAsync(JobAdminGetListInput input);
    Task<JobAdminDto> GetAsync(Guid id);
    Task ApproveJobAsync(Guid id);
    Task RejectJobAsync(Guid id);
    Task<PagedResultDto<JobApplicationAdminDto>> GetApplicationsAsync(Guid jobId, PagedAndSortedResultRequestDto input);
}

// ── Event Admin ──

public class EventAdminDto : EntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsFree { get; set; }
    public decimal? FeeAmount { get; set; }
    public int RegistrationCount { get; set; }
    public DateTime LastSubscriptionDate { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreationTime { get; set; }
}

public class EventAttendeeDto : EntityDto<Guid>
{
    public Guid AlumniId { get; set; }
    public string AlumniName { get; set; } = string.Empty;
    public string TicketCode { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
}

public class EventAdminGetListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}

public interface IEventAdminAppService : IApplicationService
{
    Task<PagedResultDto<EventAdminDto>> GetListAsync(EventAdminGetListInput input);
    Task<Informatique.Alumni.Events.AssociationEventDto> GetAsync(Guid id);
    Task<PagedResultDto<EventAttendeeDto>> GetAttendeesAsync(Guid eventId, PagedAndSortedResultRequestDto input);
    
    // CRUD methods for Admin
    Task<Informatique.Alumni.Events.AssociationEventDto> CreateEventAsync(Informatique.Alumni.Events.CreateEventDto input);
    Task<Informatique.Alumni.Events.AssociationEventDto> UpdateEventAsync(Guid id, Informatique.Alumni.Events.UpdateEventDto input);
    Task DeleteEventAsync(Guid id);
    Task PublishEventAsync(Guid id);
}

// ── Admin Dashboard ──

public class AdminDashboardOverviewDto
{
    public int PendingAlumni { get; set; }
    public int ActiveAlumni { get; set; }
    public int RejectedAlumni { get; set; }
    public int BannedAlumni { get; set; }
    public int TotalAlumni { get; set; }
    public int ActiveJobs { get; set; }
    public int UpcomingEvents { get; set; }
    public int PendingGuidanceRequests { get; set; }
    public int PendingSyndicateRequests { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<DistributionItemDto> AlumniByCollege { get; set; } = new();
    public List<DistributionItemDto> TopEmployers { get; set; } = new();
    public List<DistributionItemDto> TopLocations { get; set; } = new();
    public List<MonthlyMetricDto> MonthlyRegistrations { get; set; } = new();
    public List<ActivityItemDto> RecentActivities { get; set; } = new();
}

public class ActivityItemDto
{
    public DateTime Time { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "User", "Event", "Job"
}

public class DistributionItemDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MonthlyMetricDto
{
    public string Month { get; set; } = string.Empty; // e.g., "Jan", "Feb"
    public int Count { get; set; }
}

public interface IAdminDashboardAppService : IApplicationService
{
    Task<AdminDashboardOverviewDto> GetOverviewAsync();
}

// ── Blog Admin ──

public interface IBlogAdminAppService : IApplicationService
{
    Task<Volo.Abp.Application.Dtos.PagedResultDto<Informatique.Alumni.Magazine.BlogPostDto>> GetListAsync(Informatique.Alumni.Magazine.PostSearchInputDto input);
    Task PublishPostAsync(Guid id);
    Task UnpublishPostAsync(Guid id);
}

// ── Guidance Admin ──

public class GuidanceAdminDto : EntityDto<Guid>
{
    public Guid AlumniId { get; set; }
    public string AlumniName { get; set; } = string.Empty;
    public Guid AdvisorId { get; set; }
    public string AdvisorName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Notes { get; set; }
    public int Status { get; set; } // enum
    public DateTime CreationTime { get; set; }
}

public class GuidanceAdminGetListInput : PagedAndSortedResultRequestDto
{
    public int? Status { get; set; }
    public string? Filter { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
}

public interface IGuidanceAdminAppService : IApplicationService
{
    Task<PagedResultDto<GuidanceAdminDto>> GetListAsync(GuidanceAdminGetListInput input);
    Task ApproveRequestAsync(Guid id);
    Task RejectRequestAsync(Guid id);
}
