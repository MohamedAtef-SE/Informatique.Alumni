using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Benefits;

public interface IBenefitsAppService : IApplicationService
{
    // Academic Grants
    Task<PagedResultDto<AcademicGrantDto>> GetGrantsAsync(PagedAndSortedResultRequestDto input);
    Task<AcademicGrantDto> CreateGrantAsync(CreateUpdateAcademicGrantDto input);
    Task<AcademicGrantDto> UpdateGrantAsync(Guid id, CreateUpdateAcademicGrantDto input);
    Task DeleteGrantAsync(Guid id);

    // Commercial Discounts
    Task<PagedResultDto<CommercialDiscountDto>> GetDiscountsAsync(PagedAndSortedResultRequestDto input);
    Task<CommercialDiscountDto> CreateDiscountAsync(CreateUpdateCommercialDiscountDto input);
    Task<CommercialDiscountDto> UpdateDiscountAsync(Guid id, CreateUpdateCommercialDiscountDto input);
    Task DeleteDiscountAsync(Guid id);
}
