using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Career;

public interface ICareerServiceTypeAppService : IApplicationService
{
    Task<CareerServiceTypeDto> CreateAsync(CreateUpdateCareerServiceTypeDto input);
    Task<CareerServiceTypeDto> UpdateAsync(Guid id, CreateUpdateCareerServiceTypeDto input);
    Task DeleteAsync(Guid id);
    Task<CareerServiceTypeDto> GetAsync(Guid id);
    Task<PagedResultDto<CareerServiceTypeDto>> GetListAsync(CareerServiceTypeFilterDto input);
}
