using System;
using Volo.Abp.Application.Dtos;
using System.Collections.Generic;

namespace Informatique.Alumni.Career;

public class CareerServiceDto : FullAuditedEntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MapUrl { get; set; } = string.Empty;
    
    public bool HasFees { get; set; }
    public decimal FeeAmount { get; set; }
    public DateTime LastSubscriptionDate { get; set; }
    
    public int SubscribedCount { get; set; } // Derived or unused? Keep for now if needed.
    public List<CareerServiceTimeslotDto> Timeslots { get; set; } = new();
}

public class CareerServiceTimeslotDto : EntityDto<Guid>
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string LecturerName { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentCount { get; set; }
}

public class CreateCareerServiceDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MapUrl { get; set; } = string.Empty;
    
    public bool HasFees { get; set; }
    public decimal FeeAmount { get; set; }
    public DateTime LastSubscriptionDate { get; set; }
    
    public Guid ServiceTypeId { get; set; }
    public List<CreateCareerServiceTimeslotDto> Timeslots { get; set; } = new();
}

public class CreateCareerServiceTimeslotDto
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string LecturerName { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Capacity { get; set; }
}

public class AlumniCareerSubscriptionDto : EntityDto<Guid>
{
    public Guid CareerServiceId { get; set; }
    public Guid AlumniId { get; set; }
    public Guid TimeslotId { get; set; }
    public CareerPaymentMethod PaymentMethod { get; set; } 
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public DateTime RegistrationDate { get; set; }
}

public class BulkEmailDto
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
