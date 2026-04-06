using System;
using System.Collections.Generic; // Added for List<T>
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Guidance;

public interface IGuidanceAppService : IApplicationService
{
    Task<PagedResultDto<AdvisingRequestDto>> GetListAsync(AdvisingRequestFilterDto input);
    Task UpdateStatusAsync(Guid id, UpdateAdvisingStatusDto input);
    
    Task<AdvisingRequestDto> RequestAsync(CreateAdvisingRequestDto input);
    Task<List<AdvisorDto>> GetAvailableAdvisorsAsync(); // New
    Task<PagedResultDto<AdvisingRequestDto>> GetMyRequestsAsync(PagedAndSortedResultRequestDto input); // New
    Task<object> GetReportAsync(AdvisingReportInputDto input);

    // [New] Rule/Office Hours Management
    Task<GuidanceSessionRuleDto> GetRuleAsync(Guid branchId);
}
