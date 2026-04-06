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
using Informatique.Alumni.Profiles;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Branches;
using Volo.Abp.Identity;
using Volo.Abp.BackgroundJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Informatique.Alumni.Companies;
using Informatique.Alumni.Career;

namespace Informatique.Alumni.Events;

[Authorize]
public class EventsAppService : AlumniAppService, IEventsAppService
{
    private readonly IRepository<AssociationEvent, Guid> _eventRepository;
    private readonly IRepository<AlumniEventRegistration, Guid> _registrationRepository;
    private readonly IRepository<Company, Guid> _companyRepository;
    private readonly ActivityManager _activityManager;
    private readonly AlumniApplicationMappers _alumniMappers;

    // Added Repositories for Participant Search
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<ActivityType, Guid> _activityTypeRepository;
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IRepository<Education, Guid> _educationRepository;
    private readonly IRepository<College, Guid> _collegeRepository;
    private readonly IRepository<Major, Guid> _majorRepository;
    private readonly MembershipGuard _membershipGuard;
    private readonly IRepository<AlumniCareerSubscription, Guid> _careerSubscriptionRepository;
    private readonly IRepository<CareerService, Guid> _careerServiceRepository;

    public EventsAppService(
        IRepository<AssociationEvent, Guid> eventRepository,
        IRepository<AlumniEventRegistration, Guid> registrationRepository,
        IRepository<Company, Guid> companyRepository,
        ActivityManager activityManager,
        AlumniApplicationMappers alumniMappers,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<ActivityType, Guid> activityTypeRepository,
        IRepository<Branch, Guid> branchRepository,
        IRepository<Education, Guid> educationRepository,
        IRepository<College, Guid> collegeRepository,
        IRepository<Major, Guid> majorRepository,
        MembershipGuard membershipGuard,
        IRepository<AlumniCareerSubscription, Guid> careerSubscriptionRepository,
        IRepository<CareerService, Guid> careerServiceRepository)
    {
        _eventRepository = eventRepository;
        _registrationRepository = registrationRepository;
        _companyRepository = companyRepository;
        _activityManager = activityManager;
        _alumniMappers = alumniMappers;
        _profileRepository = profileRepository;
        _userRepository = userRepository;
        _activityTypeRepository = activityTypeRepository;
        _branchRepository = branchRepository;
        _educationRepository = educationRepository;
        _collegeRepository = collegeRepository;
        _majorRepository = majorRepository;
        _membershipGuard = membershipGuard;
        _careerSubscriptionRepository = careerSubscriptionRepository;
        _careerServiceRepository = careerServiceRepository;
    }

    // ==================== EVENT BROWSING & SEARCH ====================
    
    /// <summary>
    /// Get events list with advanced filtering
    /// Business Rules: Support EventType, EventName (text search), BranchId, DateRange filters
    /// Returns lightweight DTO for bandwidth optimization
    /// </summary>
    [AllowAnonymous] // Public can browse events
    public async Task<PagedResultDto<EventListDto>> GetListAsync(GetEventsInput input)
    {
        // 1. Get queryables
        var eventQuery = await _eventRepository.GetQueryableAsync();
        var activityTypeQuery = await _activityTypeRepository.GetQueryableAsync();
        var branchQuery = await _branchRepository.GetQueryableAsync();

        // 2. Join for names (LEFT JOIN to handle null ActivityType/Branch)
        var query = from e in eventQuery
                    join at in activityTypeQuery on e.ActivityTypeId equals at.Id into atJoin
                    from at in atJoin.DefaultIfEmpty()
                    join b in branchQuery on e.BranchId equals b.Id into bJoin
                    from b in bJoin.DefaultIfEmpty()
                    select new { Event = e, ActivityType = at, Branch = b };

        // 3. Apply filters
        if (input.ActivityTypeId.HasValue)
        {
            query = query.Where(x => x.Event.ActivityTypeId == input.ActivityTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.EventName))
        {
            var searchTerm = input.EventName.Trim();
            query = query.Where(x => x.Event.NameEn.Contains(searchTerm) || 
                                      x.Event.NameAr.Contains(searchTerm));
        }

        if (input.BranchId.HasValue)
        {
            query = query.Where(x => x.Event.BranchId == input.BranchId.Value);
        }

        if (input.DateFrom.HasValue)
        {
            query = query.Where(x => x.Event.LastSubscriptionDate >= input.DateFrom.Value);
        }

        if (input.DateTo.HasValue)
        {
            query = query.Where(x => x.Event.LastSubscriptionDate <= input.DateTo.Value);
        }

        if (input.IsPublished.HasValue)
        {
            query = query.Where(x => x.Event.IsPublished == input.IsPublished.Value);
        }

        // 4. Get total count
        var totalCount = await AsyncExecuter.CountAsync(query);

        // 5. Apply sorting and paging
        // Map frontend sorting field names to anonymous type property paths
        var sorting = input.Sorting ?? "Event.LastSubscriptionDate DESC";
        
        // Handle common frontend sorting field names
        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            sorting = input.Sorting
                .Replace("lastSubscriptionDate", "Event.LastSubscriptionDate", StringComparison.OrdinalIgnoreCase)
                .Replace("nameEn", "Event.NameEn", StringComparison.OrdinalIgnoreCase)
                .Replace("nameAr", "Event.NameAr", StringComparison.OrdinalIgnoreCase)
                .Replace("code", "Event.Code", StringComparison.OrdinalIgnoreCase);
        }
        
        query = query.OrderBy(sorting)
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount);

        // 6. Execute and map to lightweight DTO
        var result = await AsyncExecuter.ToListAsync(query);

        var items = result.Select(x => new EventListDto
        {
            Id = x.Event.Id,
            Code = x.Event.Code,
            NameEn = x.Event.NameEn,
            NameAr = x.Event.NameAr,
            ActivityTypeName = x.ActivityType?.NameEn,
            BranchName = x.Branch?.Name,
            LastSubscriptionDate = x.Event.LastSubscriptionDate,
            IsPublished = x.Event.IsPublished,
            HasFees = x.Event.HasFees,
            FeeAmount = x.Event.FeeAmount
        }).ToList();

        return new PagedResultDto<EventListDto>(totalCount, items);
    }

    /// <summary>
    /// Get event details with full agenda/program
    /// Business Rules: Include AgendaItems collection, Address, MapUrl
    /// </summary>
    [AllowAnonymous] // Public can view event details
    public async Task<EventDetailDto> GetAsync(Guid id)
    {
        // Optimization: Use WithDetailsAsync to load collections in one query
        // Note: ActivityType and Branch are not navigation properties in AssociationEvent, so we still fetch them separately if needed.
        // Or if we can, we include them. But AssociationEvent definition showed no navigation prop for Branch/ActivityType.
        // Checking AssociationEvent.cs again: 'ActivityType' IS a property! 'Branch' is NOT.
        // So we can Include ActivityType.
        
        var @event = await _eventRepository.GetAsync(id, includeDetails: true); 
        // ABP's GetAsync(includeDetails: true) usually loads all configured includes. 
        // Or using explicit WithDetailsAsync.
        // Let's use explicit to be sure about AgendaItems and Timeslots.
        
        var query = await _eventRepository.WithDetailsAsync(x => x.Agenda, x => x.Timeslots, x => x.ActivityType);
        @event = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));
        
        if (@event == null) throw new EntityNotFoundException(typeof(AssociationEvent), id);

        // Get Branch name (Manual fetch as no navigation prop)
        string? branchName = null;
        Guid? branchId = null;
        if (@event.BranchId.HasValue)
        {
            var branch = await _branchRepository.GetAsync(@event.BranchId.Value);
            branchName = branch.Name;
            branchId = branch.Id;
        }

        var dto = new EventDetailDto
        {
            Id = @event.Id,
            Code = @event.Code,
            NameEn = @event.NameEn,
            NameAr = @event.NameAr,
            Description = @event.Description,
            Location = @event.Location,
            Address = @event.Address,
            GoogleMapUrl = @event.GoogleMapUrl,
            ActivityTypeId = @event.ActivityTypeId,
            ActivityTypeName = @event.ActivityType?.NameEn,
            BranchId = branchId,
            BranchName = branchName,
            LastSubscriptionDate = @event.LastSubscriptionDate,
            IsPublished = @event.IsPublished,
            HasFees = @event.HasFees,
            FeeAmount = @event.FeeAmount,
            
            // Include full agenda/program (sorted by date and time)
            AgendaItems = @event.Agenda
                .OrderBy(a => a.Date)
                .ThenBy(a => a.StartTime)
                .Select(a => new EventAgendaItemDto
                {
                    Id = a.Id,
                    EventId = a.EventId,
                    Date = a.Date,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    ActivityName = a.ActivityName,
                    Place = a.Place,
                    Description = a.Description
                }).ToList(),
            
            // Include timeslots (sorted by start time)
            Timeslots = @event.Timeslots
                .OrderBy(t => t.StartTime)
                .Select(t => new EventTimeslotDto
                {
                    Id = t.Id,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    Capacity = t.Capacity
                }).ToList()
        };

        // Check if current user is registered
        if (CurrentUser.IsAuthenticated)
        {
            try
            {
                var alumniId = await GetCurrentAlumniProfileIdAsync();
                var registration = await _registrationRepository.FirstOrDefaultAsync(x => x.AlumniId == alumniId && x.EventId == id);
                if (registration != null)
                {
                    dto.MyRegistration = _alumniMappers.MapToDto(registration);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to fetch registration status for event detail view");
            }
        }

        return dto;
    }


    // ==================== PARTICIPANT MANAGEMENT ====================

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task<PagedResultDto<ActivityParticipantDto>> GetParticipantsAsync(ActivityParticipantFilterDto input)
    {
        // 1. Get Queryables
        var registrationQuery = await _registrationRepository.GetQueryableAsync();
        var eventQuery = await _eventRepository.WithDetailsAsync(x => x.Timeslots);
        var profileQuery = await _profileRepository.GetQueryableAsync();
        var userQuery = await _userRepository.GetQueryableAsync();
        var activityTypeQuery = await _activityTypeRepository.GetQueryableAsync();
        var educationQuery = await _educationRepository.GetQueryableAsync();
        
        // 2. Base Query (Correct Join: Registration -> Profile -> User)
        var query = from r in registrationQuery
                    join e in eventQuery on r.EventId equals e.Id
                    join p in profileQuery on r.AlumniId equals p.Id
                    join u in userQuery on p.UserId equals u.Id
                    join at in activityTypeQuery on e.ActivityTypeId equals at.Id into atJoin
                    from at in atJoin.DefaultIfEmpty()
                    where r.Status != RegistrationStatus.Cancelled 
                    select new { r, e, p, u, at };

        // 3. Filters
        if (input.BranchId.HasValue)
        {
            query = query.Where(x => x.e.BranchId == input.BranchId);
        }
        
        if (input.ActivityTypeId.HasValue)
        {
            query = query.Where(x => x.e.ActivityTypeId == input.ActivityTypeId);
        }

        if (!input.ActivityName.IsNullOrEmpty())
        {
            query = query.Where(x => x.e.NameEn.Contains(input.ActivityName!) || x.e.NameAr.Contains(input.ActivityName!));
        }

        if (!input.Location.IsNullOrEmpty())
        {
            query = query.Where(x => x.e.Location.Contains(input.Location!));
        }

        if (input.FromDate.HasValue)
        {
            query = query.Where(x => x.e.Timeslots.Any(t => t.StartTime >= input.FromDate.Value));
        }
        if (input.ToDate.HasValue)
        {
             query = query.Where(x => x.e.Timeslots.Any(t => t.EndTime <= input.ToDate.Value));
        }

        // 4. Paging & Sorting
        var count = await AsyncExecuter.CountAsync(query);
        var list = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.r.CreationTime).PageBy(input.SkipCount, input.MaxResultCount));

        // 5. Fetch Additional Info
        var profileIds = list.Select(x => x.p.Id).Distinct().ToList();
        var educations = await AsyncExecuter.ToListAsync(educationQuery.Where(e => profileIds.Contains(e.AlumniProfileId)));
        var educationsByProfile = educations.ToLookup(e => e.AlumniProfileId);
        
        var collegeIds = educations.Where(e => e.CollegeId.HasValue).Select(e => e.CollegeId.Value).Distinct().ToList();
        var majorIds = educations.Where(e => e.MajorId.HasValue).Select(e => e.MajorId.Value).Distinct().ToList();
        
        var colleges = await AsyncExecuter.ToListAsync((await _collegeRepository.GetQueryableAsync()).Where(x => collegeIds.Contains(x.Id)));
        var majors = await AsyncExecuter.ToListAsync((await _majorRepository.GetQueryableAsync()).Where(x => majorIds.Contains(x.Id)));

        var resultDtos = new List<ActivityParticipantDto>();

        foreach (var item in list)
        {
             var bestEdu = educationsByProfile[item.p.Id].OrderByDescending(e => e.GraduationYear).FirstOrDefault();
             
             var dto = new ActivityParticipantDto
             {
                 Id = item.r.Id,
                 AlumniId = item.r.AlumniId,
                 AlumniName = item.u.Name + " " + item.u.Surname,
                 MobileNumber = item.p.MobileNumber ?? "",
                 JobTitle = item.p.JobTitle,
                 Company = item.p.Company,
                 NationalId = item.p.NationalId,
                 Address = item.p.Address,
                 CreationTime = item.r.CreationTime,
                 CollegeName = bestEdu?.CollegeId != null ? colleges.FirstOrDefault(c => c.Id == bestEdu.CollegeId)?.Name : "",
                 MajorName = bestEdu?.MajorId != null ? majors.FirstOrDefault(m => m.Id == bestEdu.MajorId)?.Name : "",
                 GraduationYear = bestEdu?.GraduationYear ?? 0,
                 EventName = item.e.NameEn,
                 ActivityTypeName = item.at?.NameEn ?? "",
                 TimeslotStart = item.r.TimeslotId.HasValue 
                                 ? item.e.Timeslots.FirstOrDefault(t => t.Id == item.r.TimeslotId)?.StartTime ?? DateTime.MinValue 
                                 : item.e.Timeslots.Min(t => t.StartTime),
                 TimeslotEnd = item.r.TimeslotId.HasValue 
                               ? item.e.Timeslots.FirstOrDefault(t => t.Id == item.r.TimeslotId)?.EndTime ?? DateTime.MinValue
                               : item.e.Timeslots.Max(t => t.EndTime),
                 Location = item.e.Location,
                 Status = item.r.Status,
                 PaymentMethod = item.r.PaymentMethod ?? "",
                 PaidAmount = item.r.PaidAmount
             };
             resultDtos.Add(dto);
        }

        return new PagedResultDto<ActivityParticipantDto>(count, resultDtos);
    }

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task RemoveParticipantAsync(Guid subscriptionId, string cancellationReason)
    {
        var manager = LazyServiceProvider.LazyGetRequiredService<EventSubscriptionManager>();
        await manager.ForceCancelAsync(subscriptionId, cancellationReason);
    }

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task ApproveRegistrationAsync(Guid id)
    {
        var registration = await _registrationRepository.GetAsync(id);
        registration.Confirm();
        await _registrationRepository.UpdateAsync(registration);
    }

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task RejectRegistrationAsync(Guid id, string reason)
    {
        var registration = await _registrationRepository.GetAsync(id);
        registration.Cancel();
        // Optionially log the reason elsewhere if needed, or update entity to store reason
        await _registrationRepository.UpdateAsync(registration);
    }
    public async Task<PagedResultDto<AssociationEventDto>> GetEventsAsync(PagedAndSortedResultRequestDto input)
    {
        var count = await _eventRepository.GetCountAsync();
        var query = await _eventRepository.WithDetailsAsync(x => x.Timeslots, x => x.ParticipatingCompanies);
        
        // Non-admins only see published events
        if (!await AuthorizationService.IsGrantedAsync(AlumniPermissions.Events.Manage))
        {
            query = query.Where(x => x.IsPublished);
        }

        // Sorting adjusted (Date -> LastSubscriptionDate or CreationTime)
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(input.Sorting ?? "LastSubscriptionDate desc").PageBy(input.SkipCount, input.MaxResultCount));
        
        return new PagedResultDto<AssociationEventDto>(count, _alumniMappers.MapToDtos(list));
    }

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task<AssociationEventDto> CreateEventAsync(CreateEventDto input)
    {
        var timeslots = input.Timeslots.Select(x => (x.StartTime, x.EndTime, x.Capacity)).ToList();
        var companies = input.ParticipatingCompanies.Select(x => (x.CompanyId, x.ParticipationTypeId)).ToList();

        var @event = await _activityManager.CreateAsync(
            input.NameAr,
            input.NameEn,
            input.Code,
            input.Description,
            input.Location,
            input.Address,
            input.GoogleMapUrl,
            input.HasFees,
            input.FeeAmount,
            input.LastSubscriptionDate,
            input.BranchId,
            timeslots,
            companies
        );

        return _alumniMappers.MapToDto(@event);
    }

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task<AssociationEventDto> UpdateEventAsync(Guid id, UpdateEventDto input)
    {
        var @event = await _eventRepository.GetAsync(id);
        
        await _eventRepository.EnsureCollectionLoadedAsync(@event, x => x.Timeslots);
        await _eventRepository.EnsureCollectionLoadedAsync(@event, x => x.ParticipatingCompanies);

        var timeslots = input.Timeslots.Select(x => (x.StartTime, x.EndTime, x.Capacity)).ToList();
        var companies = input.ParticipatingCompanies.Select(x => (x.CompanyId, x.ParticipationTypeId)).ToList();

        await _activityManager.UpdateAsync(
            @event,
            input.NameAr,
            input.NameEn,
            input.Code,
            input.Description,
            input.Location,
            input.Address,
            input.GoogleMapUrl,
            input.HasFees,
            input.FeeAmount,
            input.LastSubscriptionDate,
            input.BranchId,
            timeslots,
            companies
        );

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
        await _eventRepository.EnsureCollectionLoadedAsync(@event, x => x.Timeslots); // Check timeslots
        @event.Publish();
        await _eventRepository.UpdateAsync(@event);
    }

    /* Agenda Removed
    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task AddAgendaItemAsync(Guid eventId, CreateAgendaItemDto input)
    {
       // Removed
    }

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task RemoveAgendaItemAsync(Guid eventId, Guid agendaItemId)
    {
       // Removed
    }
    */

    [Authorize(AlumniPermissions.Events.Register)]
    public async Task<AlumniEventRegistrationDto> RegisterAsync(Guid id)
    {
        // 1. Business Rule: Active membership required
        await _membershipGuard.CheckAsync();
        
        var @event = await _eventRepository.GetAsync(id);
        var alumniId = await GetCurrentAlumniProfileIdAsync();
        var profile = await _profileRepository.GetAsync(alumniId);
        
        // 2. Prevent double registration
        if (await _registrationRepository.AnyAsync(x => x.AlumniId == alumniId && x.EventId == id))
        {
            throw new UserFriendlyException("You are already registered for this event.");
        }

        // 3. Capacity Check
        await _eventRepository.EnsureCollectionLoadedAsync(@event, x => x.Timeslots);
        var currentCount = await _registrationRepository.CountAsync(x => x.EventId == id && x.Status != RegistrationStatus.Cancelled);
        var totalCapacity = @event.Timeslots.Sum(x => x.Capacity);
        
        if (!@event.IsPublished || currentCount >= totalCapacity)
        {
             throw new UserFriendlyException("Event is not open for registration or has reached maximum capacity.");
        }

        // 4. Payment Enforcement (Real-world wallet deduction)
        decimal? paidAmount = null;
        string? paymentMethod = null;

        if (@event.HasFees && @event.FeeAmount > 0)
        {
            if (profile.WalletBalance < @event.FeeAmount)
            {
                throw new UserFriendlyException($"Insufficient wallet balance. This event requires {@event.FeeAmount} EGP, but your balance is only {profile.WalletBalance} EGP.");
            }

            profile.DeductWallet(@event.FeeAmount.Value);
            await _profileRepository.UpdateAsync(profile);
            
            paidAmount = @event.FeeAmount;
            paymentMethod = "Wallet";
        }

        // 5. Create Registration
        var ticketCode = GuidGenerator.Create().ToString("N").ToUpper();
        var registration = new AlumniEventRegistration(
            id: GuidGenerator.Create(), 
            alumniId: alumniId, 
            eventId: id, 
            ticketCode: ticketCode,
            paidAmount: paidAmount,
            paymentMethod: paymentMethod
        );
        
        await _registrationRepository.InsertAsync(registration);
        
        var dto = _alumniMappers.MapToDto(registration);
        dto.QrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={ticketCode}";
        
        return dto;
    }

    public async Task<List<AlumniEventRegistrationDto>> GetMyRegistrationsAsync()
    {
        var alumniId = await GetCurrentAlumniProfileIdAsync();
        
        // 1. Fetch Event Registrations
        var eventRegistrations = await _registrationRepository.GetListAsync(x => x.AlumniId == alumniId);
        var eventIds = eventRegistrations.Select(x => x.EventId).Distinct().ToList();
        var events = await _eventRepository.GetListAsync(x => eventIds.Contains(x.Id));
        var eventDict = events.ToDictionary(x => x.Id);

        // 2. Fetch Career Workshop Subscriptions
        var careerSubscriptions = await _careerSubscriptionRepository.GetListAsync(x => x.AlumniId == alumniId && x.PaymentStatus != CareerPaymentStatus.Cancelled);
        var careerServiceIds = careerSubscriptions.Select(x => x.CareerServiceId).Distinct().ToList();
        var careerServices = await _careerServiceRepository.GetListAsync(x => careerServiceIds.Contains(x.Id));
        var careerDict = careerServices.ToDictionary(x => x.Id);

        var allDtos = new List<AlumniEventRegistrationDto>();

        // Map Events
        foreach (var reg in eventRegistrations)
        {
            var dto = _alumniMappers.MapToDto(reg);
            dto.Type = RegistrationType.Event;
            dto.QrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={dto.TicketCode}";
            
            if (eventDict.TryGetValue(reg.EventId, out var evt))
            {
                dto.EventName = evt.NameEn;
                dto.EventDate = evt.LastSubscriptionDate;
                dto.Location = evt.Location;
            }
            allDtos.Add(dto);
        }

        // Map Workshops
        foreach (var sub in careerSubscriptions)
        {
            var dto = new AlumniEventRegistrationDto
            {
                Id = sub.Id,
                AlumniId = sub.AlumniId,
                EventId = sub.CareerServiceId, // Reuse EventId for ServiceId
                TicketCode = sub.Id.ToString("N").ToUpper().Substring(0, 8), // Generate a short code for workshops if missing
                Status = RegistrationStatus.Confirmed,
                Type = RegistrationType.Workshop,
                CreationTime = sub.RegistrationDate,
                QrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={sub.Id}"
            };

            if (careerDict.TryGetValue(sub.CareerServiceId, out var svc))
            {
                dto.EventName = svc.NameEn;
                dto.EventDate = svc.LastSubscriptionDate;
                dto.Location = svc.MapUrl; // Use map URL as location hint
                
                // Try to find timeslot info if needed, but for list view service info is enough
                var timeslot = svc.Timeslots?.FirstOrDefault(t => t.Id == sub.TimeslotId);
                if (timeslot != null)
                {
                    dto.LecturerName = timeslot.LecturerName;
                    dto.Room = timeslot.Room;
                    dto.Location = $"{timeslot.Room} - {timeslot.Address}";
                }
            }
            allDtos.Add(dto);
        }
        
        return allDtos.OrderByDescending(x => x.CreationTime).ToList();
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

    // ========== Schedule/Agenda Management ==========

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task<EventAgendaItemDto> CreateScheduleItemAsync(Guid eventId, CreateAgendaItemDto input)
    {
        // 1. Load Event with Agenda Items for conflict detection
        var @event = await _eventRepository.GetAsync(eventId, includeDetails: false);
        
        // 2. Branch Security Check: Employees can only manage events from their branch
        var currentUserBranchId = CurrentUser.FindClaim("BranchId")?.Value;
        if (@event.BranchId.HasValue && currentUserBranchId != null)
        {
            var branchId = Guid.Parse(currentUserBranchId);
            if (@event.BranchId != branchId)
            {
                throw new BusinessException("Alumni:Unauthorized")
                    .WithData("Reason", "Employee can only manage events from their assigned branch")
                    .WithData("EventBranchId", @event.BranchId)
                    .WithData("UserBranchId", branchId);
            }
        }

        // 3. Call Domain Manager with Conflict Detection
        var agendaItem = await _activityManager.AddAgendaItemAsync(
            @event,
            input.Date,
            input.TimeFrom,
            input.TimeTo,
            input.ActivityName,
            input.Place,
            input.Description);

        // 4. Update Event
        await _eventRepository.UpdateAsync(@event);

        // 5. Return DTO
        return new EventAgendaItemDto
        {
            Id = agendaItem.Id,
            EventId = agendaItem.EventId,
            Date = agendaItem.Date,
            StartTime = agendaItem.StartTime,
            EndTime = agendaItem.EndTime,
            ActivityName = agendaItem.ActivityName,
            Place = agendaItem.Place,
            Description = agendaItem.Description
        };
    }

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task RemoveScheduleItemAsync(Guid eventId, Guid agendaItemId)
    {
        // 1. Load Event
        var @event = await _eventRepository.GetAsync(eventId, includeDetails: false);

        // 2. Branch Security Check
        var currentUserBranchId = CurrentUser.FindClaim("BranchId")?.Value;
        if (@event.BranchId.HasValue && currentUserBranchId != null)
        {
            var branchId = Guid.Parse(currentUserBranchId);
            if (@event.BranchId != branchId)
            {
                throw new BusinessException("Alumni:Unauthorized")
                    .WithData("Reason", "Employee can only manage events from their assigned branch");
            }
        }

        // 3. Remove Agenda Item
        @event.RemoveAgendaItem(agendaItemId);

        // 4. Update Event
        await _eventRepository.UpdateAsync(@event);
    }
    
    [AllowAnonymous]
    public async Task<IRemoteStreamContent> GetAgendaPdfAsync(Guid eventId)
    {
        var @event = await _eventRepository.GetAsync(eventId);
        await _eventRepository.EnsureCollectionLoadedAsync(@event, x => x.Agenda);
        
        var sortedAgenda = @event.Agenda
            .OrderBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .ToList();

        // 1. Generate HTML
        var htmlContent = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; padding: 20px; }}
                    header {{ text-align: center; margin-bottom: 30px; border-bottom: 2px solid #ccc; padding-bottom: 20px; }}
                    h1 {{ color: #333; margin: 0; }}
                    .meta {{ color: #666; margin-top: 5px; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                    th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
                    th {{ background-color: #f2f2f2; font-weight: bold; }}
                    tr:nth-child(even) {{ background-color: #f9f9f9; }}
                    .date-header {{ background-color: #e9ecef; font-weight: bold; text-align: center; }}
                </style>
            </head>
            <body>
                <header>
                    <h1>{@event.NameEn}</h1>
                    <div class='meta'>{@event.NameAr}</div>
                    <div class='meta'>Location: {@event.Location}</div>
                </header>
                
                <h2>Event Agenda</h2>
                
                <table>
                    <thead>
                        <tr>
                            <th>Time</th>
                            <th>Activity</th>
                            <th>Description</th>
                            <th>Place</th>
                        </tr>
                    </thead>
                    <tbody>";

        var currentDate = DateTime.MinValue;
        foreach (var item in sortedAgenda)
        {
            if (item.Date.Date != currentDate)
            {
                currentDate = item.Date.Date;
                htmlContent += $@"
                        <tr>
                            <td colspan='4' class='date-header'>{currentDate:dddd, MMMM dd, yyyy}</td>
                        </tr>";
            }
            
            htmlContent += $@"
                        <tr>
                            <td>{item.StartTime:hh\:mm} - {item.EndTime:hh\:mm}</td>
                            <td>{item.ActivityName}</td>
                            <td>{item.Description ?? "-"}</td>
                            <td>{item.Place ?? "-"}</td>
                        </tr>";
        }

        htmlContent += @"
                    </tbody>
                </table>
            </body>
            </html>";

        // 2. Generate PDF using PuppeteerSharp
        var browserFetcher = new PuppeteerSharp.BrowserFetcher();
        await browserFetcher.DownloadAsync(); // Ensure browser is downloaded
        
        using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(new PuppeteerSharp.LaunchOptions
        {
            Headless = true
        });
        
        using var page = await browser.NewPageAsync();
        await page.SetContentAsync(htmlContent);
        var pdfStream = await page.PdfStreamAsync();
        
        // 3. Return Content
        return new Volo.Abp.Content.RemoteStreamContent(
            pdfStream, 
            $"{@event.Code}_Agenda.pdf", 
            "application/pdf");
    }

    // ========== Bulk Email Management ==========

    [Authorize(AlumniPermissions.Events.Manage)]
    public async Task SendEmailToParticipantsAsync(SendEventEmailInputDto input)
    {
        // 1. Validate event exists
        var @event = await _eventRepository.GetAsync(input.EventId);

        // 2. Branch Security Check
        var currentUserBranchId = CurrentUser.FindClaim("BranchId")?.Value;
        if (@event.BranchId.HasValue && currentUserBranchId != null)
        {
            var branchId = Guid.Parse(currentUserBranchId);
            if (@event.BranchId != branchId)
            {
                throw new BusinessException("Alumni:Unauthorized")
                    .WithData("Reason", "Employee can only send emails for events from their assigned branch");
            }
        }

        // 3. Upload attachments to blob storage
        var attachmentBlobNames = new List<string>();
        
        if (input.Attachments != null && input.Attachments.Any())
        {
            var blobContainer = LazyServiceProvider.LazyGetRequiredService<IBlobContainer<EventAttachmentsBlobContainer>>();
            
            foreach (var attachment in input.Attachments)
            {
                // Validate file extension (security check)
                var extension = Path.GetExtension(attachment.FileName)?.ToLowerInvariant();
                var allowedExtensions = new[] { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", 
                    ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".avi", ".mov" };
                
                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    throw new BusinessException("Alumni:InvalidFileType")
                        .WithData("FileName", attachment.FileName)
                        .WithData("AllowedTypes", "Word, Excel, PowerPoint, PDF, Images (JPG/PNG/GIF), Videos (MP4/AVI/MOV)");
                }
                
                // Generate unique blob name with timestamp to avoid conflicts
                var blobName = $"{Guid.NewGuid()}_{DateTime.Now:yyyyMMddHHmmss}{extension}";

                // Upload to blob storage
                using (var stream = attachment.GetStream())
                {
                    await blobContainer.SaveAsync(blobName, stream, overrideExisting: false);
                }

                attachmentBlobNames.Add(blobName);
                Logger.LogInformation($"Uploaded attachment: {attachment.FileName} → {blobName}");
            }
            
            Logger.LogInformation($"Successfully uploaded {attachmentBlobNames.Count} attachments to blob storage");
        }

        // 4. Create job arguments
        var jobArgs = new EventEmailJobArgs
        {
            EventId = input.EventId,
            Subject = input.Subject,
            Body = input.Body,
            AttachmentBlobNames = attachmentBlobNames,
            SenderUserId = CurrentUser.Id ?? Guid.Empty
        };

        // 5. Enqueue background job
        var backgroundJobManager = LazyServiceProvider.LazyGetRequiredService<IBackgroundJobManager>();
        await backgroundJobManager.EnqueueAsync(jobArgs);

        Logger.LogInformation($"Email job enqueued for EventId: {input.EventId}, Attachments: {attachmentBlobNames.Count}");
        
        // Return immediately - processing happens in background
    }

}
