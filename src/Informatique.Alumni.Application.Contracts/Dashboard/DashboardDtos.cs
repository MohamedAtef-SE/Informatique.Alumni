using System;
using Volo.Abp.Application.Dtos;
using Informatique.Alumni.Trips;

namespace Informatique.Alumni.Dashboard;

public class DailyStatsDto : EntityDto<Guid>
{
    public DailyStatsDto() { }
    public DateTime Date { get; set; }
    public int TotalAlumniCount { get; set; }
    public double EmploymentRate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveJobsCount { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class AlumniTripDto : FullAuditedEntityDto<Guid>
{
    public AlumniTripDto() { }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxCapacity { get; set; }
    public decimal PricePerPerson { get; set; }
    public bool IsActive { get; set; }
}

public class TripRequestDto : FullAuditedEntityDto<Guid>
{
    public TripRequestDto() { }
    public Guid TripId { get; set; }
    public Guid AlumniId { get; set; }
    public int GuestCount { get; set; }
    public decimal TotalAmount { get; set; }
    public TripRequestStatus Status { get; set; }
    public int TotalParticipants { get; set; }
}
