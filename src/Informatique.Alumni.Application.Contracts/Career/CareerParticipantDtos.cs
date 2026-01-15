using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Career;

public class CareerParticipantFilterDto
{
    public Guid BranchId { get; set; } // Mandatory
    public Guid ServiceTypeId { get; set; } // Mandatory
    
    public string? ServiceName { get; set; } // Optional
    public DateTime? FromDate { get; set; } // Optional
    public DateTime? ToDate { get; set; } // Optional
    public string? Location { get; set; } // Optional
}

public class CareerParticipantDto : EntityDto<Guid>
{
    // Subscription details
    public Guid SubscriptionId { get; set; }
    public CareerPaymentStatus PaymentStatus { get; set; }
    
    // Alumni details
    public Guid AlumniId { get; set; }
    public string AlumniName { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    
    // Service details
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceTypeName { get; set; } = string.Empty;
    
    // Timeslot details
    public DateTime TimeslotDate { get; set; }
    public TimeSpan TimeslotStartTime { get; set; }
    public TimeSpan TimeslotEndTime { get; set; }
    public string Location { get; set; } = string.Empty;
}
