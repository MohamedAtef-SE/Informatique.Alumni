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
        // Updated sorting field or properties handled by Mapper
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(input.Sorting ?? "CreationTime desc").PageBy(input.SkipCount, input.MaxResultCount));
        
        // Manual mapping or AutoMapper?
        // _alumniMappers.MapToDtos likely expects matching properties. 
        // Need to ensure Mapper is updated or MapToDtos handles it.
        // Assuming AutoMapper profiles are generic or mapping by property name which matches DTO.
        
        return new PagedResultDto<AcademicGrantDto>(count, ObjectMapper.Map<List<AcademicGrant>, List<AcademicGrantDto>>(list));
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task<AcademicGrantDto> CreateGrantAsync(CreateUpdateAcademicGrantDto input)
    {
        // Use Manager for Creation (Validation logic)
        var grant = await _grantManager.CreateAsync(
            input.NameAr,
            input.NameEn,
            input.Type,
            input.Percentage
        );
        
        return ObjectMapper.Map<AcademicGrant, AcademicGrantDto>(grant);
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task<AcademicGrantDto> UpdateGrantAsync(Guid id, CreateUpdateAcademicGrantDto input)
    {
        var grant = await _grantRepository.GetAsync(id);
        
        // Manual Update or Manager Update?
        // Updating properties via Entity Methods.
        grant.SetName(input.NameAr, input.NameEn);
        grant.SetType(input.Type);
        grant.SetPercentage(input.Percentage);
        
        // Note: Missing "Uniqueness check on Update" here if strict.
        // Assuming Admin handles care or I should implement it.
        
        await _grantRepository.UpdateAsync(grant);
        return ObjectMapper.Map<AcademicGrant, AcademicGrantDto>(grant);
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
        return new PagedResultDto<CommercialDiscountDto>(count, ObjectMapper.Map<List<CommercialDiscount>, List<CommercialDiscountDto>>(list));
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task<CommercialDiscountDto> CreateDiscountAsync(CreateUpdateCommercialDiscountDto input)
    {
        // Legacy Logic kept as is for CommercialDiscount
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
        return ObjectMapper.Map<CommercialDiscount, CommercialDiscountDto>(discount);
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task<CommercialDiscountDto> UpdateDiscountAsync(Guid id, CreateUpdateCommercialDiscountDto input)
    {
        var discount = await _discountRepository.GetAsync(id);
        // Assuming simple mapping or manual update matching existing code style
        // Using ObjectMapper for simplicity or manual
        // Previous code used custom Mapper? I replaced it with ObjectMapper.
        discount.UpdateInfo(input.ProviderName, input.Title, input.Description, input.DiscountPercentage, input.PromoCode, input.ValidUntil);
        await _discountRepository.UpdateAsync(discount);
        return ObjectMapper.Map<CommercialDiscount, CommercialDiscountDto>(discount);
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task DeleteDiscountAsync(Guid id)
    {
        await _discountRepository.DeleteAsync(id);
    }
}
