using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Career;

public interface ICareerAppService : IApplicationService
{
    Task<PagedResultDto<CareerServiceDto>> GetServicesAsync(PagedAndSortedResultRequestDto input);
    Task<CareerServiceDto> CreateServiceAsync(CreateCareerServiceDto input);
    Task<CareerServiceDto> UpdateServiceAsync(Guid id, CreateCareerServiceDto input);
    Task DeleteServiceAsync(Guid id);
    Task<CareerServiceDto> GetServiceAsync(Guid id);
    
    // Subscription Methods
    Task<AlumniCareerSubscriptionDto> SubscribeAsync(Guid serviceId, Guid timeslotId, CareerPaymentMethod paymentMethod);
    Task CancelSubscriptionAsync(Guid serviceId);
    
    Task SendBulkEmailAsync(Guid serviceId, BulkEmailDto input);
    
    // Employee Dashboard - Participant Management
    Task<List<CareerParticipantDto>> GetParticipantsAsync(CareerParticipantFilterDto filter);
    Task RemoveParticipantAsync(Guid subscriptionId, string cancellationReason);
}
