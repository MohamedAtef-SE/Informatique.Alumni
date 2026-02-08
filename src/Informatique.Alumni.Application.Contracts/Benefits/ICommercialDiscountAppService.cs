using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Benefits;

public interface ICommercialDiscountAppService : IApplicationService
{
    Task<PagedResultDto<CommercialDiscountDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<CommercialDiscountDto> CreateAsync(CreateUpdateCommercialDiscountDto input);
    Task<CommercialDiscountDto> UpdateAsync(Guid id, CreateUpdateCommercialDiscountDto input);
    Task DeleteAsync(Guid id);
}
