using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Informatique.Alumni.Events;

[Authorize]
public class EventsAppService : AlumniAppService, IEventsAppService
{
    private readonly IRepository<AssociationEvent, Guid> _eventRepository;
    private readonly IRepository<AlumniEventRegistration, Guid> _registrationRepository;
    private readonly IRepository<Company, Guid> _companyRepository;
    private readonly AlumniApplicationMappers _alumniMappers;

    public EventsAppService(
        IRepository<AssociationEvent, Guid> eventRepository,
        IRepository<AlumniEventRegistration, Guid> registrationRepository,
        IRepository<Company, Guid> companyRepository,
        AlumniApplicationMappers alumniMappers)
    {
        _eventRepository = eventRepository;
        _registrationRepository = registrationRepository;
        _companyRepository = companyRepository;
        _alumniMappers = alumniMappers;
    }

    public async Task<PagedResultDto<AssociationEventDto>> GetEventsAsync(PagedAndSortedResultRequestDto input)
    {
        var count = await _eventRepository.GetCountAsync();
        var query = await _eventRepository.GetQueryableAsync();
        
        // Non-admins only see published events
        if (!await AuthorizationService.IsGrantedAsync(AlumniPermissions.Events.Manage))
        {
            query = query.Where(x => x.IsPublished);
        }

        var list = await AsyncExecuter.ToListAsync(query.OrderBy(input.Sorting ?? "Date desc").PageBy(input.SkipCount, input.MaxResultCount));
        return new PagedResultDto<AssociationEventDto>(count, _alumniMappers.MapToDtos(list));
    }

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task<AssociationEventDto> CreateEventAsync(CreateUpdateEventDto input)
    {
        var @event = new AssociationEvent(
            GuidGenerator.Create(),
            input.Title,
            input.Description,
            input.Date,
            input.Location,
            input.Capacity
        );
        await _eventRepository.InsertAsync(@event);
        return _alumniMappers.MapToDto(@event);
    }

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task<AssociationEventDto> UpdateEventAsync(Guid id, CreateUpdateEventDto input)
    {
        var @event = await _eventRepository.GetAsync(id);
        @event.Update(input.Title, input.Description, input.Date, input.Location, input.Capacity);
        await _eventRepository.UpdateAsync(@event);
        return _alumniMappers.MapToDto(@event);
    }

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task DeleteEventAsync(Guid id)
    {
        await _eventRepository.DeleteAsync(id);
    }

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task PublishEventAsync(Guid id)
    {
        var @event = await _eventRepository.GetAsync(id);
        @event.Publish();
        await _eventRepository.UpdateAsync(@event);
    }

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task AddAgendaItemAsync(Guid eventId, CreateAgendaItemDto input)
    {
        var @event = await _eventRepository.GetAsync(eventId);
        await _eventRepository.EnsureCollectionLoadedAsync(@event, x => x.Agenda);
        @event.AddAgendaItem(GuidGenerator.Create(), input.Title, input.StartTime, input.EndTime, input.Speaker);
        await _eventRepository.UpdateAsync(@event);
    }

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task RemoveAgendaItemAsync(Guid eventId, Guid agendaItemId)
    {
        var @event = await _eventRepository.GetAsync(eventId);
        await _eventRepository.EnsureCollectionLoadedAsync(@event, x => x.Agenda);
        @event.RemoveAgendaItem(agendaItemId);
        await _eventRepository.UpdateAsync(@event);
    }

    [Authorize(AlumniPermissions.Events.Register)]
    public async Task<AlumniEventRegistrationDto> RegisterAsync(Guid eventId)
    {
        var @event = await _eventRepository.GetAsync(eventId);
        var alumniId = CurrentUser.GetId();
        
        // Prevent double registration
        if (await _registrationRepository.AnyAsync(x => x.AlumniId == alumniId && x.EventId == eventId))
        {
            throw new UserFriendlyException("You are already registered for this event.");
        }

        // Use domain method to check if registration is allowed
        var currentCount = await _registrationRepository.CountAsync(x => x.EventId == eventId && x.Status != RegistrationStatus.Cancelled);
        if (!@event.CanRegister(currentCount))
        {
            throw new UserFriendlyException("Event is not open for registration or has reached maximum capacity.");
        }

        var ticketCode = GuidGenerator.Create().ToString("N").ToUpper();
        var registration = new AlumniEventRegistration(GuidGenerator.Create(), alumniId, eventId, ticketCode);
        
        await _registrationRepository.InsertAsync(registration);
        
        var dto = _alumniMappers.MapToDto(registration);
        dto.QrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={ticketCode}";
        
        // Simulation: Sending email with ticket
        return dto;
    }

    public async Task<List<AlumniEventRegistrationDto>> GetMyRegistrationsAsync()
    {
        var alumniId = CurrentUser.GetId();
        var list = await _registrationRepository.GetListAsync(x => x.AlumniId == alumniId);
        var dtos = _alumniMappers.MapToDtos(list);
        
        foreach (var dto in dtos)
        {
            dto.QrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={dto.TicketCode}";
        }
        
        return dtos;
    }

    [Authorize(AlumniPermissions.Events.VerifyTicket)]
    public async Task<AlumniEventRegistrationDto> VerifyTicketAsync(string ticketCode)
    {
        var registration = await _registrationRepository.FirstOrDefaultAsync(x => x.TicketCode == ticketCode);
        if (registration == null)
        {
            throw new UserFriendlyException("Invalid ticket code.");
        }

        registration.MarkAsAttended();
        await _registrationRepository.UpdateAsync(registration);
        
        return _alumniMappers.MapToDto(registration);
    }

    public async Task<List<CompanyDto>> GetCompaniesAsync()
    {
        var list = await _companyRepository.GetListAsync();
        return _alumniMappers.MapToDtos(list);
    }

    [Authorize(AlumniPermissions.Companies.Manage)]
    public async Task<CompanyDto> CreateCompanyAsync(CompanyDto input)
    {
        var company = new Company(GuidGenerator.Create(), input.Name, input.Logo, input.Website, input.Industry);
        await _companyRepository.InsertAsync(company);
        return _alumniMappers.MapToDto(company);
    }
}
