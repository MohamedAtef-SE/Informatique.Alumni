using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Events;

public class CompanyDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
}

public class AssociationEventDto : FullAuditedEntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsPublished { get; set; }
    public List<EventAgendaItemDto> Agenda { get; set; } = new();
}

public class EventAgendaItemDto : EntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Speaker { get; set; } = string.Empty;
}

public class AlumniEventRegistrationDto : FullAuditedEntityDto<Guid>
{
    public Guid AlumniId { get; set; }
    public Guid EventId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }
    public string QrCodeUrl { get; set; } = string.Empty;
}

public class CreateUpdateEventDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
}

public class CreateAgendaItemDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Speaker { get; set; } = string.Empty;
}
