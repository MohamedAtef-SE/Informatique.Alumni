using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Benefits;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.BenefitsManage)]
public class BenefitsAdminAppService : AlumniAppService, IBenefitsAdminAppService
{
    private readonly IRepository<AcademicGrant, Guid> _grantRepository;
    private readonly IRepository<CommercialDiscount, Guid> _discountRepository;

    public BenefitsAdminAppService(
        IRepository<AcademicGrant, Guid> grantRepository,
        IRepository<CommercialDiscount, Guid> discountRepository)
    {
        _grantRepository = grantRepository;
        _discountRepository = discountRepository;
    }

    public async Task<PagedResultDto<AcademicGrantAdminDto>> GetGrantsAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _grantRepository.GetQueryableAsync();
        var totalCount = queryable.Count();

        queryable = queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var items = queryable.ToList().Select(g => new AcademicGrantAdminDto
        {
            Id = g.Id,
            NameAr = g.NameAr,
            NameEn = g.NameEn,
            Type = g.Type,
            Percentage = g.Percentage
        }).ToList();

        return new PagedResultDto<AcademicGrantAdminDto>(totalCount, items);
    }

    public async Task<PagedResultDto<CommercialDiscountAdminDto>> GetDiscountsAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _discountRepository.GetQueryableAsync();
        var totalCount = queryable.Count();

        queryable = queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var items = queryable.ToList().Select(d => new CommercialDiscountAdminDto
        {
            Id = d.Id,
            ProviderName = d.ProviderName,
            Title = d.Title,
            DiscountPercentage = d.DiscountPercentage,
            ValidUntil = d.ValidUntil
        }).ToList();

        return new PagedResultDto<CommercialDiscountAdminDto>(totalCount, items);
    }

    public async Task DeleteGrantAsync(Guid id)
    {
        await _grantRepository.DeleteAsync(id);
    }

    public async Task DeleteDiscountAsync(Guid id)
    {
        await _discountRepository.DeleteAsync(id);
    }
}
