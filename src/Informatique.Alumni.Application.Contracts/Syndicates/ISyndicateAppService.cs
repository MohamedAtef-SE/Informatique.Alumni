using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Informatique.Alumni.Syndicates;

public interface ISyndicateAppService : IApplicationService
{
    Task<List<SyndicateDto>> GetSyndicatesAsync();
    Task<SyndicateDto> CreateSyndicateAsync(CreateUpdateSyndicateDto input);
    
    Task<SyndicateSubscriptionDto> ApplyAsync(ApplySyndicateDto input);
    Task<SyndicateSubscriptionDto> GetMySubscriptionAsync(Guid syndicateId);
    Task<List<SyndicateSubscriptionDto>> GetMyApplicationsAsync();
    
    Task UploadDocumentAsync(Guid subscriptionId, UploadSyndicateDocDto input);
    Task<SyndicateSubscriptionDto> PayAsync(Guid subscriptionId);
    Task<SyndicateSubscriptionDto> GetApplicationDetailsAsync(Guid subscriptionId);
    
    Task BatchUpdateStatusAsync(BatchUpdateStatusDto input);
    Task<SyndicateSubscriptionDto> CreateRequestByEmployeeAsync(CreateSyndicateRequestDto input);
    Task<SyndicateSubscriptionDto> UpdateRequestStatusAsync(Guid id, SyndicateRequestStatus newStatus);
    Task<IRemoteStreamContent> GetDocumentAsync(Guid subscriptionId, Guid documentId);
    Task<PagedResultDto<SyndicateSubscriptionDto>> GetListAsync(SyndicateRequestFilterDto input);
}
