using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Events;

public class CompanyDto : EntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string LogoBlobName { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
}

public class AssociationEventDto : FullAuditedEntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? GoogleMapUrl { get; set; }
    
    public bool HasFees { get; set; }
    public decimal? FeeAmount { get; set; }
    public DateTime LastSubscriptionDate { get; set; }
    public bool IsPublished { get; set; }
    public Guid? BranchId { get; set; }

    public List<EventTimeslotDto> Timeslots { get; set; } = new();
    public List<EventParticipatingCompanyDto> ParticipatingCompanies { get; set; } = new();
}

public class EventTimeslotDto : EntityDto<Guid>
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
}

public class EventParticipatingCompanyDto : EntityDto<Guid>
{
    public Guid CompanyId { get; set; }
    public Guid ParticipationTypeId { get; set; }
    public CompanyDto? Company { get; set; }
    public ParticipationTypeDto? ParticipationType { get; set; }
}

public class AlumniEventRegistrationDto : FullAuditedEntityDto<Guid>
{
    public Guid AlumniId { get; set; }
    public Guid EventId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }
    public string QrCodeUrl { get; set; } = string.Empty;
    
    // Properties for UI/Optimization (Bulk loaded)
    public string? EventName { get; set; }
    public DateTime? EventDate { get; set; }
    public string? Location { get; set; }
}

public class CreateEventDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? GoogleMapUrl { get; set; }
    
    public bool HasFees { get; set; }
    public decimal? FeeAmount { get; set; }
    public DateTime LastSubscriptionDate { get; set; }
    public Guid? BranchId { get; set; }

    public List<CreateTimeslotDto> Timeslots { get; set; } = new();

    public List<CreateCompanyParticipationDto> ParticipatingCompanies { get; set; } = new();
}

public class UpdateEventDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? GoogleMapUrl { get; set; }
    
    public bool HasFees { get; set; }
    public decimal? FeeAmount { get; set; }
    public DateTime LastSubscriptionDate { get; set; }
    public Guid? BranchId { get; set; }

    public List<CreateTimeslotDto> Timeslots { get; set; } = new();
    public List<CreateCompanyParticipationDto> ParticipatingCompanies { get; set; } = new();
}

public class CreateTimeslotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
}

public class CreateCompanyParticipationDto
{
    public Guid CompanyId { get; set; }
    public Guid ParticipationTypeId { get; set; }
}

public class ParticipationTypeDto : FullAuditedEntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

public class CreateParticipationTypeDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

public class UpdateParticipationTypeDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

public class CreateAgendaItemDto
{
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public TimeSpan TimeFrom { get; set; }
    
    [Required]
    public TimeSpan TimeTo { get; set; }
    
    [Required]
    [MaxLength(256)]
    public string ActivityName {get; set; } = string.Empty;
    
    [MaxLength(256)]
    public string? Place { get; set; }
    
    [MaxLength(2000)]
    public string? Description { get; set; }
}

public class EventAgendaItemDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public string? Place { get; set; }
    public string? Description { get; set; }
}
