using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Events;

public class ActivityParticipantFilterDto : PagedAndSortedResultRequestDto
{
    public Guid? BranchId { get; set; }
    public Guid? ActivityTypeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? ActivityName { get; set; }
    public string? Location { get; set; }
}

public class ActivityParticipantDto
{
    public Guid Id { get; set; } // Registration Id
    public Guid AlumniId { get; set; }
    public string AlumniName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    
    public string EventName { get; set; } = string.Empty;
    public string ActivityTypeName { get; set; } = string.Empty;
    public DateTime TimeslotStart { get; set; }
    public DateTime TimeslotEnd { get; set; }
    public string Location { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }
    
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal? PaidAmount { get; set; }
}
