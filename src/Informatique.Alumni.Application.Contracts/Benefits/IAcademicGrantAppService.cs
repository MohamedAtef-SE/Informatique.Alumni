using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Benefits;

public interface IAcademicGrantAppService : IApplicationService
{
    Task<PagedResultDto<AcademicGrantDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<AcademicGrantDto> CreateAsync(CreateUpdateAcademicGrantDto input);
    Task<AcademicGrantDto> UpdateAsync(Guid id, CreateUpdateAcademicGrantDto input);
    Task DeleteAsync(Guid id);
}
