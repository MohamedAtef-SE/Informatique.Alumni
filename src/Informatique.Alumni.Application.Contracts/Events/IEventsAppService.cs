using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Events;

public interface IEventsAppService : IApplicationService
{
    // Event Management
    Task<PagedResultDto<AssociationEventDto>> GetEventsAsync(PagedAndSortedResultRequestDto input);
    Task<AssociationEventDto> CreateEventAsync(CreateUpdateEventDto input);
    Task<AssociationEventDto> UpdateEventAsync(Guid id, CreateUpdateEventDto input);
    Task DeleteEventAsync(Guid id);
    Task PublishEventAsync(Guid id);
    
    // Agenda Management
    Task AddAgendaItemAsync(Guid eventId, CreateAgendaItemDto input);
    Task RemoveAgendaItemAsync(Guid eventId, Guid agendaItemId);

    // Registration
    Task<AlumniEventRegistrationDto> RegisterAsync(Guid eventId);
    Task<List<AlumniEventRegistrationDto>> GetMyRegistrationsAsync();
    
    // Gatekeeping
    Task<AlumniEventRegistrationDto> VerifyTicketAsync(string ticketCode);

    // Companies
    Task<List<CompanyDto>> GetCompaniesAsync();
    Task<CompanyDto> CreateCompanyAsync(CompanyDto input);
}
