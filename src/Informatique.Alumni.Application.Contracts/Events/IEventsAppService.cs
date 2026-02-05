using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Content;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Events;

public interface IEventsAppService : IApplicationService
{
    // Event Management
    // Event Browsing & Search
    Task<PagedResultDto<EventListDto>> GetListAsync(GetEventsInput input);
    Task<EventDetailDto> GetAsync(Guid id);
    
    // Event Management (Admin)
    Task<PagedResultDto<AssociationEventDto>> GetEventsAsync(PagedAndSortedResultRequestDto input);
    Task<AssociationEventDto> CreateEventAsync(CreateEventDto input);
    Task<AssociationEventDto> UpdateEventAsync(Guid id, UpdateEventDto input);
    Task DeleteEventAsync(Guid id);
    Task PublishEventAsync(Guid id);
    
    
    // Agenda/Schedule Management
    Task<EventAgendaItemDto> CreateScheduleItemAsync(Guid eventId, CreateAgendaItemDto input);
    Task RemoveScheduleItemAsync(Guid eventId, Guid agendaItemId);
    Task<IRemoteStreamContent> GetAgendaPdfAsync(Guid eventId);


    // Registration (Register prefix for proper ABP routing: POST /api/app/events/{id}/register)
    Task<AlumniEventRegistrationDto> RegisterAsync(Guid id);
    Task<List<AlumniEventRegistrationDto>> GetMyRegistrationsAsync();
    
    // Gatekeeping
    Task<AlumniEventRegistrationDto> VerifyTicketAsync(string ticketCode);

    // Companies
    Task<List<CompanyDto>> GetCompaniesAsync();
    // Participant Management (Employees)
    Task<PagedResultDto<ActivityParticipantDto>> GetParticipantsAsync(ActivityParticipantFilterDto input);
    Task RemoveParticipantAsync(Guid subscriptionId, string cancellationReason);
    
    // Bulk Email
    Task SendEmailToParticipantsAsync(SendEventEmailInputDto input);
}
