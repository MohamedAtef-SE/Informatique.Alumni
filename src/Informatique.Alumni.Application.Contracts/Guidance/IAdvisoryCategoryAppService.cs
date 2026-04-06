using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Guidance;

public interface IAdvisoryCategoryAppService : IApplicationService
{
    Task<AdvisoryCategoryDto> CreateAsync(CreateUpdateAdvisoryCategoryDto input);
    Task<AdvisoryCategoryDto> UpdateAsync(Guid id, CreateUpdateAdvisoryCategoryDto input);
    Task DeleteAsync(Guid id);
    Task<AdvisoryCategoryDto> GetAsync(Guid id);
    Task<PagedResultDto<AdvisoryCategoryDto>> GetListAsync(AdvisoryCategoryFilterDto input);
    Task<List<AdvisoryCategoryDto>> GetActiveListAsync();
}
