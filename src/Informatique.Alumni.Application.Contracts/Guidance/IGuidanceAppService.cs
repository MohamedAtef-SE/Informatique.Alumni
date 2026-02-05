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
    
    Task<AdvisingRequestDto> CreateRequestAsync(CreateAdvisingRequestDto input);
    Task<List<AdvisorDto>> GetAvailableAdvisorsAsync(); // New
    Task<PagedResultDto<AdvisingRequestDto>> GetMyRequestsAsync(PagedAndSortedResultRequestDto input); // New
    Task<object> GetReportAsync(AdvisingReportInputDto input);
}

public class AdvisorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}
