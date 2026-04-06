using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Health;

public interface IMedicalCategoryAppService : IApplicationService
{
    Task<PagedResultDto<MedicalCategoryDto>> GetListAsync(GetMedicalPartnersInput input);
    Task<MedicalCategoryDto> GetAsync(Guid id);
    Task<MedicalCategoryDto> CreateAsync(CreateUpdateMedicalCategoryDto input);
    Task<MedicalCategoryDto> UpdateAsync(Guid id, CreateUpdateMedicalCategoryDto input);
    Task DeleteAsync(Guid id);
}
