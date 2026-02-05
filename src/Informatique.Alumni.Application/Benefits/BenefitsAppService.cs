using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Informatique.Alumni.Magazine; // For HtmlSanitizerService - kept for CommercialDiscount if needed
// Note: AcademicGrant properties changed, logic updated. using AcademicGrantManager for creation.

namespace Informatique.Alumni.Benefits;

[Authorize]
public class BenefitsAppService : AlumniAppService, IBenefitsAppService
{
    private readonly IRepository<AcademicGrant, Guid> _grantRepository;
    private readonly IRepository<CommercialDiscount, Guid> _discountRepository;
    private readonly AcademicGrantManager _grantManager;
    private readonly HtmlSanitizerService _sanitizer;
    private readonly AlumniApplicationMappers _alumniMappers;

    public BenefitsAppService(
        IRepository<AcademicGrant, Guid> grantRepository,
        IRepository<CommercialDiscount, Guid> discountRepository,
        AcademicGrantManager grantManager,
        HtmlSanitizerService sanitizer,
        AlumniApplicationMappers alumniMappers)
    {
        _grantRepository = grantRepository;
        _discountRepository = discountRepository;
        _grantManager = grantManager;
        _sanitizer = sanitizer;
        _alumniMappers = alumniMappers;
    }

    [Authorize(AlumniPermissions.Benefits.View)]
    public async Task<PagedResultDto<AcademicGrantDto>> GetGrantsAsync(PagedAndSortedResultRequestDto input)
    {
        var count = await _grantRepository.GetCountAsync();
        var query = await _grantRepository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(input.Sorting ?? "CreationTime desc").PageBy(input.SkipCount, input.MaxResultCount));
        
        return new PagedResultDto<AcademicGrantDto>(count, _alumniMappers.MapToDtos(list));
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task<AcademicGrantDto> CreateGrantAsync(CreateUpdateAcademicGrantDto input)
    {
        var grant = await _grantManager.CreateAsync(
            input.NameAr,
            input.NameEn,
            input.Type,
            input.Percentage
        );
        
        return _alumniMappers.MapToDto(grant);
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task<AcademicGrantDto> UpdateGrantAsync(Guid id, CreateUpdateAcademicGrantDto input)
    {
        var grant = await _grantRepository.GetAsync(id);
        
        grant.SetName(input.NameAr, input.NameEn);
        grant.SetType(input.Type);
        grant.SetPercentage(input.Percentage);
        
        await _grantRepository.UpdateAsync(grant);
        return _alumniMappers.MapToDto(grant);
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task DeleteGrantAsync(Guid id)
    {
        await _grantRepository.DeleteAsync(id);
    }

    [Authorize(AlumniPermissions.Benefits.View)]
    public async Task<PagedResultDto<CommercialDiscountDto>> GetDiscountsAsync(PagedAndSortedResultRequestDto input)
    {
        var count = await _discountRepository.GetCountAsync();
        var query = await _discountRepository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(input.Sorting ?? "CreationTime desc").PageBy(input.SkipCount, input.MaxResultCount));
        return new PagedResultDto<CommercialDiscountDto>(count, _alumniMappers.MapToDtos(list));
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task<CommercialDiscountDto> CreateDiscountAsync(CreateUpdateCommercialDiscountDto input)
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
    public async Task<CommercialDiscountDto> UpdateDiscountAsync(Guid id, CreateUpdateCommercialDiscountDto input)
    {
        var discount = await _discountRepository.GetAsync(id);
        discount.UpdateInfo(input.ProviderName, input.Title, input.Description, input.DiscountPercentage, input.PromoCode, input.ValidUntil);
        await _discountRepository.UpdateAsync(discount);
        return _alumniMappers.MapToDto(discount);
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task DeleteDiscountAsync(Guid id)
    {
        await _discountRepository.DeleteAsync(id);
    }
}
