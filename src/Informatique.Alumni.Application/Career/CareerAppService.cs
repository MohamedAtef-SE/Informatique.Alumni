using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.BlobContainers;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BlobStoring;
using Volo.Abp.Authorization;
using System.Linq.Dynamic.Core;
using Informatique.Alumni.Profiles;

namespace Informatique.Alumni.Career;
using Informatique.Alumni.Admin;
using Informatique.Alumni.Branches;

[Authorize(AlumniPermissions.Career.Default)]
public class CareerAppService : ApplicationService, ICareerAppService
{
    private readonly IRepository<CareerService, Guid> _serviceRepository;
    private readonly IRepository<AlumniCareerSubscription, Guid> _subscriptionRepository;
    private readonly CareerServiceManager _careerManager;
    private readonly CareerSubscriptionManager _subscriptionManager;
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly AlumniApplicationMappers _alumniMappers;
    private readonly IBlobContainer<AlumniBlobContainer> _blobContainer;
    private readonly Volo.Abp.EventBus.Local.ILocalEventBus _localEventBus;
    private readonly IRepository<CareerServiceType, Guid> _serviceTypeRepository;
    private readonly IRepository<Branch, Guid> _branchRepository;

    public CareerAppService(
        IRepository<CareerService, Guid> serviceRepository,
        IRepository<AlumniCareerSubscription, Guid> subscriptionRepository,
        CareerServiceManager careerManager,
        CareerSubscriptionManager subscriptionManager,
        IBackgroundJobManager backgroundJobManager,
        AlumniApplicationMappers alumniMappers,
        IBlobContainer<AlumniBlobContainer> blobContainer,
        Volo.Abp.EventBus.Local.ILocalEventBus localEventBus,
        IRepository<CareerServiceType, Guid> serviceTypeRepository,
        IRepository<Branch, Guid> branchRepository)
    {
        _serviceRepository = serviceRepository;
        _subscriptionRepository = subscriptionRepository;
        _careerManager = careerManager;
        _subscriptionManager = subscriptionManager;
        _backgroundJobManager = backgroundJobManager;
        _alumniMappers = alumniMappers;
        _blobContainer = blobContainer;
        _localEventBus = localEventBus;
        _serviceTypeRepository = serviceTypeRepository;
        _branchRepository = branchRepository;
    }

    public async Task<PagedResultDto<CareerServiceDto>> GetServicesAsync(PagedAndSortedResultRequestDto input)
    {
        var count = await _serviceRepository.GetCountAsync();
        var query = await _serviceRepository.WithDetailsAsync(x => x.Timeslots, x => x.ServiceType, x => x.Branch);
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime desc" : input.Sorting;
        
        // Fix: Sort before Paging
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).PageBy(input.SkipCount, input.MaxResultCount));
        
        var dtos = _alumniMappers.MapToDtos(list);
        
        var serviceIds = list.Select(s => s.Id).ToList();
        var subCounts = await AsyncExecuter.ToListAsync(
            (await _subscriptionRepository.GetQueryableAsync())
            .Where(r => serviceIds.Contains(r.CareerServiceId))
            .GroupBy(r => r.CareerServiceId)
            .Select(g => new { ServiceId = g.Key, Count = g.Count() })
        );

        foreach (var dto in dtos)
        {
            dto.SubscribedCount = subCounts.FirstOrDefault(c => c.ServiceId == dto.Id)?.Count ?? 0;
        }
        
        return new PagedResultDto<CareerServiceDto>(count, dtos);
    }

    [Authorize(AlumniPermissions.Career.Manage)]
    public async Task<CareerServiceDto> CreateServiceAsync(CreateCareerServiceDto input)
    {
        // Manager expects List of Tuples for Timeslots
        var timeslots = input.Timeslots.Select(t => (
            t.Date, 
            t.StartTime, 
            t.EndTime, 
            t.LecturerName, 
            t.Room, 
            t.Address, 
            t.Capacity
        )).ToList();

        var service = await _careerManager.CreateAsync(
            input.NameAr,
            input.NameEn,
            input.Code,
            input.Description,
            input.HasFees,
            input.FeeAmount,
            input.LastSubscriptionDate,
            input.ServiceTypeId,
            input.BranchId,
            timeslots
        );

        service.SetMapUrl(input.MapUrl);
        await _serviceRepository.InsertAsync(service);
        
        return _alumniMappers.MapToDto(service);
    }

    [Authorize(AlumniPermissions.Career.Manage)]
    public async Task<CareerServiceDto> UpdateServiceAsync(Guid id, CreateCareerServiceDto input)
    {
        var query = await _serviceRepository.WithDetailsAsync(x => x.Timeslots, x => x.ServiceType, x => x.Branch);
        var service = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id))
            ?? throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(CareerService), id);
        
        await _careerManager.UpdateAsync(
            service,
            input.NameAr,
            input.NameEn,
            input.Code,
            input.Description,
            input.HasFees,
            input.FeeAmount,
            input.LastSubscriptionDate,
            input.ServiceTypeId,
            input.BranchId
        );
        
        service.SetMapUrl(input.MapUrl);
        await _serviceRepository.UpdateAsync(service);
        
        return _alumniMappers.MapToDto(service);
    }

    public async Task<CareerServiceDto> GetAsync(Guid id)
    {
        var query = await _serviceRepository.WithDetailsAsync(x => x.Timeslots, x => x.ServiceType, x => x.Branch);
        var service = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id))
            ?? throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(CareerService), id);
            
        var dto = _alumniMappers.MapToDto(service);
        dto.SubscribedCount = await _subscriptionRepository.CountAsync(s => s.CareerServiceId == id);
        return dto;
    }

    [Authorize(AlumniPermissions.Career.Manage)]
    public async Task DeleteServiceAsync(Guid id)
    {
        await _serviceRepository.DeleteAsync(id);
    }

    [Authorize(AlumniPermissions.Career.Manage)]
    public async Task<CareerLookupsDto> GetLookupsAsync()
    {
        var activeTypes = await _serviceTypeRepository.GetListAsync(x => x.IsActive);
        var branches = await _branchRepository.GetListAsync();

        return new CareerLookupsDto
        {
            ServiceTypes = activeTypes.Select(x => new CareerLookupItemDto
            {
                Id = x.Id,
                NameAr = x.NameAr,
                NameEn = x.NameEn
            }).ToList(),
            Branches = branches.Select(x => new CareerLookupItemDto
            {
                Id = x.Id,
                NameAr = x.Name,
                NameEn = x.Name
            }).ToList()
        };
    }

    [Authorize(AlumniPermissions.Career.Register)]
    public async Task<AlumniCareerSubscriptionDto> SubscribeAsync(Guid serviceId, Guid timeslotId, CareerPaymentMethod paymentMethod)
    {
        var subscription = await _subscriptionManager.SubscribeAsync(
            serviceId, 
            CurrentUser.Id ?? throw new AbpAuthorizationException("User must be authenticated"),
            timeslotId,
            paymentMethod
        );
        
        return _alumniMappers.MapToDto(subscription);
    }

    [Authorize(AlumniPermissions.Career.Register)]
    public async Task CancelSubscriptionAsync(Guid serviceId)
    {
        await _subscriptionManager.CancelSubscriptionAsync(
            serviceId, 
            CurrentUser.Id ?? throw new AbpAuthorizationException("User must be authenticated")
        );
    }

    [Authorize(AlumniPermissions.Career.Manage)]
    public async Task SendBulkEmailAsync(Guid serviceId, BulkEmailDto input)
    {
        // Re-implement simplified or keep logic using new entities
        var service = await _serviceRepository.GetAsync(serviceId);
        
        // Implementation similar to previous logic but pointing to Service
        // For brevity in this turn, I will leave it empty or basic log if needed.
        // The previous implementation used _backgroundJobManager.EnqueueAsync
        
        // Re-enabling logic:
        await _backgroundJobManager.EnqueueAsync(
            new CareerServiceEmailArgs 
            { 
               ServiceId = serviceId,
               Subject = input.Subject,
               Body = input.Body,
               SenderId = CurrentUser.Id
            }
        );
    }

    // Employee Dashboard - Participant Management
    [Authorize(AlumniPermissions.Career.Manage)]
    public async Task<List<CareerParticipantDto>> GetParticipantsAsync(CareerParticipantFilterDto filter)
    {
        // NOTE: This is a simplified version for build purposes
        // TODO: Optimize with proper LINQ projections
        
        // 1. Get all subscriptions with filtered services
        var subscriptionsQuery = await _subscriptionRepository.GetQueryableAsync();
        var servicesQuery = await _serviceRepository.GetQueryableAsync();
        
        // Apply service filters
        servicesQuery = servicesQuery.Where(s => s.BranchId == filter.BranchId && s.ServiceTypeId == filter.ServiceTypeId);
        
        if (!string.IsNullOrWhiteSpace(filter.ServiceName))
        {
            servicesQuery = servicesQuery.Where(s => s.NameEn.Contains(filter.ServiceName) || s.NameAr.Contains(filter.ServiceName));
        }

        var serviceIds = await AsyncExecuter.ToListAsync(servicesQuery.Select(s => s.Id));
        var subscriptions = await AsyncExecuter.ToListAsync(
            subscriptionsQuery.Where(sub => serviceIds.Contains(sub.CareerServiceId))
        );

        // 2. Load related data
        var timeslotRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<CareerServiceTimeslot, Guid>>();
        var timeslotsQuery = await timeslotRepo.GetQueryableAsync();
        var timeslots = await AsyncExecuter.ToListAsync(timeslotsQuery);
        var services = await AsyncExecuter.ToListAsync(servicesQuery);
        
        var serviceTypeRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<CareerServiceType, Guid>>();
        var serviceTypes = await AsyncExecuter.ToListAsync(await serviceTypeRepo.GetQueryableAsync());
        
        // Apply date filters
        if (filter.FromDate.HasValue || filter.ToDate.HasValue)
        {
            var filteredTimeslotIds = timeslots
                .Where(ts => (!filter.FromDate.HasValue || ts.Date >= filter.FromDate.Value) &&
                            (!filter.ToDate.HasValue || ts.Date <= filter.ToDate.Value))
                .Select(ts => ts.Id)
                .ToList();
            subscriptions = subscriptions.Where(s => filteredTimeslotIds.Contains(s.TimeslotId)).ToList();
        }
        
        // Apply location filter
        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            var filteredTimeslotIds = timeslots
                .Where(ts => ts.Room.Contains(filter.Location) || ts.Address.Contains(filter.Location))
                .Select(ts => ts.Id)
                .ToList();
            subscriptions = subscriptions.Where(s => filteredTimeslotIds.Contains(s.TimeslotId)).ToList();
        }

        // 3. Get alumni details
        var profileRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<AlumniProfile, Guid>>();
        var alumniIds = subscriptions.Select(s => s.AlumniId).Distinct().ToList();
        var profiles = await AsyncExecuter.ToListAsync(
            (await profileRepo.GetQueryableAsync()).Where(p => alumniIds.Contains(p.Id))
        );

        // 4. Get user names
        var userIds = profiles.Select(p => p.UserId).Distinct().ToList();
        var userRepo = LazyServiceProvider.LazyGetRequiredService<Volo.Abp.Identity.IIdentityUserRepository>();
        var users = await userRepo.GetListByIdsAsync(userIds);
        var userDict = users.ToDictionary(u => u.Id, u => u.Name + " " + u.Surname);

        // 5. Get education details (latest graduation)
        var eduRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<Education, Guid>>();
        var educations = await AsyncExecuter.ToListAsync(
            (await eduRepo.GetQueryableAsync()).Where(e => alumniIds.Contains(e.AlumniProfileId))
        );
        var latestEducations = educations
            .GroupBy(e => e.AlumniProfileId)
            .Select(g => g.OrderByDescending(e => e.GraduationYear).First())
            .ToDictionary(e => e.AlumniProfileId);

        var collegeRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<College, Guid>>();
        var majorRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<Major, Guid>>();
        var colleges = await AsyncExecuter.ToListAsync(await collegeRepo.GetQueryableAsync());
        var majors = await AsyncExecuter.ToListAsync(await majorRepo.GetQueryableAsync());
        var collegeDict = colleges.ToDictionary(c => c.Id, c => c.Name);
        var majorDict = majors.ToDictionary(m => m.Id, m => m.Name);

        // 6. Build DTOs
        var result = new List<CareerParticipantDto>();
        foreach (var sub in subscriptions)
        {
            var service = services.FirstOrDefault(s => s.Id == sub.CareerServiceId);
            var timeslot = timeslots.FirstOrDefault(ts => ts.Id == sub.TimeslotId);
            var profile = profiles.FirstOrDefault(p => p.Id == sub.AlumniId);
            if (service == null || timeslot == null || profile == null) continue;

            var serviceType = serviceTypes.FirstOrDefault(st => st.Id == service.ServiceTypeId);
            var education = latestEducations.GetValueOrDefault(profile.Id);

            string collegeName = "";
            string majorName = "";
            if (education != null)
            {
                if (education.CollegeId.HasValue && collegeDict.ContainsKey(education.CollegeId.Value))
                    collegeName = collegeDict[education.CollegeId.Value];
                if (education.MajorId.HasValue && majorDict.ContainsKey(education.MajorId.Value))
                    majorName = majorDict[education.MajorId.Value];
            }

            result.Add(new CareerParticipantDto
            {
                SubscriptionId = sub.Id,
                PaymentStatus = sub.PaymentStatus,
                AlumniId = profile.UserId,
                AlumniName = userDict.TryGetValue(profile.UserId, out var name) ? name : "Unknown",
                GraduationYear = education?.GraduationYear ?? 0,
                CollegeName = collegeName,
                MajorName = majorName,
                ServiceName = service.NameEn,
                ServiceTypeName = serviceType?.NameEn ?? "",
                TimeslotDate = timeslot.Date,
                TimeslotStartTime = timeslot.StartTime,
                TimeslotEndTime = timeslot.EndTime,
                Location = timeslot.Room
            });
        }

        return result;
    }

    [Authorize(AlumniPermissions.Career.Manage)]
    public async Task RemoveParticipantAsync(Guid subscriptionId, string cancellationReason)
    {
        // 1. Call Manager to Cancel (with refund logic)
        var eventData = await _subscriptionManager.ForceCancelAsync(subscriptionId, cancellationReason);

        // 2. Publish Event for Notification
        await _localEventBus.PublishAsync(eventData);
    }
}
