using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Events;

/// <summary>
/// Input DTO for searching/filtering events with specific criteria
/// </summary>
public class GetEventsInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// Filter by event/activity type
    /// </summary>
    public Guid? ActivityTypeId { get; set; }
    
    /// <summary>
    /// Text search in event name (Arabic or English)
    /// </summary>
    public string? EventName { get; set; }
    
    /// <summary>
    /// Filter by campus/branch
    /// </summary>
    public Guid? BranchId { get; set; }
    
    /// <summary>
    /// Filter events with subscription deadline starting from this date
    /// </summary>
    public DateTime? DateFrom { get; set; }
    
    /// <summary>
    /// Filter events with subscription deadline ending at this date
    /// </summary>
    public DateTime? DateTo { get; set; }
    
    /// <summary>
    /// Filter by published status (null = all, true = published only, false = unpublished only)
    /// </summary>
    public bool? IsPublished { get; set; }
}

/// <summary>
/// Lightweight DTO for event list view (bandwidth optimization)
/// </summary>
public class EventListDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ActivityTypeName { get; set; }
    public string? BranchName { get; set; }
    public DateTime LastSubscriptionDate { get; set; }
    public bool IsPublished { get; set; }
    public bool HasFees { get; set; }
    public decimal? FeeAmount { get; set; }
}

/// <summary>
/// Detailed DTO for event details view including full agenda/program
/// </summary>
public class EventDetailDto : EventListDto
{
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? GoogleMapUrl { get; set; }
    public Guid? ActivityTypeId { get; set; }
    public Guid? BranchId { get; set; }
    
    /// <summary>
    /// Event Program/Agenda - Daily schedule with activities
    /// </summary>
    public List<EventAgendaItemDto> AgendaItems { get; set; } = new();
    
    /// <summary>
    /// Available time slots for registration
    /// </summary>
    public List<EventTimeslotDto> Timeslots { get; set; } = new();
}
