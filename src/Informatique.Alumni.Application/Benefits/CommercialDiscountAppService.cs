using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Benefits;

[Authorize]
public class CommercialDiscountAppService : AlumniAppService, ICommercialDiscountAppService
{
    private readonly IRepository<CommercialDiscount, Guid> _discountRepository;
    private readonly AlumniApplicationMappers _alumniMappers;

    public CommercialDiscountAppService(
        IRepository<CommercialDiscount, Guid> discountRepository,
        AlumniApplicationMappers alumniMappers)
    {
        _discountRepository = discountRepository;
        _alumniMappers = alumniMappers;
    }

    [Authorize(AlumniPermissions.Benefits.View)]
    public async Task<PagedResultDto<CommercialDiscountDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var count = await _discountRepository.GetCountAsync();
        var query = await _discountRepository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(input.Sorting ?? "CreationTime desc").PageBy(input.SkipCount, input.MaxResultCount));
        return new PagedResultDto<CommercialDiscountDto>(count, _alumniMappers.MapToDtos(list));
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task<CommercialDiscountDto> CreateAsync(CreateUpdateCommercialDiscountDto input)
    {
        var discount = new CommercialDiscount(
            GuidGenerator.Create(),
            input.CategoryId,
            input.ProviderName,
            input.Title,
            input.Description,
            input.DiscountPercentage,
            input.PromoCode,
            input.ValidUntil
        );
        await _discountRepository.InsertAsync(discount);
        return _alumniMappers.MapToDto(discount);
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task<CommercialDiscountDto> UpdateAsync(Guid id, CreateUpdateCommercialDiscountDto input)
    {
        var discount = await _discountRepository.GetAsync(id);
        discount.UpdateInfo(input.ProviderName, input.Title, input.Description, input.DiscountPercentage, input.PromoCode, input.ValidUntil);
        await _discountRepository.UpdateAsync(discount);
        return _alumniMappers.MapToDto(discount);
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _discountRepository.DeleteAsync(id);
    }
}
