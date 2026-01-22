using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Guidance;

public interface IGuidanceAppService : IApplicationService
{
    Task<PagedResultDto<AdvisingRequestDto>> GetListAsync(AdvisingRequestFilterDto input);
    Task UpdateStatusAsync(Guid id, UpdateAdvisingStatusDto input);
    
    Task<AdvisingRequestDto> CreateRequestAsync(CreateAdvisingRequestDto input);
    Task<object> GetReportAsync(AdvisingReportInputDto input);
}
