using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Events;

public interface IActivityTypeAppService : IApplicationService
{
    Task<PagedResultDto<ActivityTypeDto>> GetListAsync(ActivityTypeFilterDto input);
    Task<ActivityTypeDto> GetAsync(Guid id);
    Task<ActivityTypeDto> CreateAsync(CreateActivityTypeDto input);
    Task<ActivityTypeDto> UpdateAsync(Guid id, UpdateActivityTypeDto input);
    Task DeleteAsync(Guid id);
}
