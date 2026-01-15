using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Guidance;

public interface IGuidanceAppService : IApplicationService
{
    // Availability (Advisor/Employee)
    // Task<GuidanceSessionRuleDto> CreateRuleAsync(CreateUpdateGuidanceSessionRuleDto input);
    // Task<List<GuidanceSessionRuleDto>> GetRulesAsync(Guid advisorId);
    // Task DeleteRuleAsync(Guid id);

    // Booking (Alumni)
    Task<AdvisingRequestDto> BookSessionAsync(BookSessionDto input);
    Task<PagedResultDto<AdvisingRequestDto>> GetMyRequestsAsync(PagedAndSortedResultRequestDto input);
    
    // Management (Advisor)
    Task<AdvisingRequestDto> UpdateRequestStatusAsync(Guid id, AdvisingRequestStatus status);
}
