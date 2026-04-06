using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Health;

public class MedicalCategoryAppService : AlumniAppService, IMedicalCategoryAppService
{
    private readonly IRepository<MedicalCategory, Guid> _categoryRepository;
    private readonly AlumniApplicationMappers _alumniMappers;

    public MedicalCategoryAppService(
        IRepository<MedicalCategory, Guid> categoryRepository,
        AlumniApplicationMappers alumniMappers)
    {
        _categoryRepository = categoryRepository;
        _alumniMappers = alumniMappers;
    }

    [Authorize(AlumniPermissions.Health.Manage)]
    public async Task<MedicalCategoryDto> CreateAsync(CreateUpdateMedicalCategoryDto input)
    {
        var category = new MedicalCategory(
            GuidGenerator.Create(),
            input.NameEn,
            input.NameAr,
            input.BaseType
        )
        {
            IsActive = input.IsActive
        };

        await _categoryRepository.InsertAsync(category);
        return _alumniMappers.MapToDto(category);
    }

    [Authorize(AlumniPermissions.Health.Manage)]
    public async Task<MedicalCategoryDto> UpdateAsync(Guid id, CreateUpdateMedicalCategoryDto input)
    {
        var category = await _categoryRepository.GetAsync(id);

        category.NameEn = input.NameEn;
        category.NameAr = input.NameAr;
        category.BaseType = input.BaseType;
        category.IsActive = input.IsActive;

        await _categoryRepository.UpdateAsync(category);
        return _alumniMappers.MapToDto(category);
    }

    [Authorize(AlumniPermissions.Health.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _categoryRepository.DeleteAsync(id);
    }

    [AllowAnonymous]
    public async Task<MedicalCategoryDto> GetAsync(Guid id)
    {
        var category = await _categoryRepository.GetAsync(id);
        return _alumniMappers.MapToDto(category);
    }

    [AllowAnonymous]
    public async Task<PagedResultDto<MedicalCategoryDto>> GetListAsync(GetMedicalPartnersInput input)
    {
        var queryable = await _categoryRepository.GetQueryableAsync();

        if (!string.IsNullOrEmpty(input.FilterText))
        {
            queryable = queryable.Where(c => c.NameEn.Contains(input.FilterText) || c.NameAr.Contains(input.FilterText));
        }

        var count = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable.OrderBy(input.Sorting ?? nameof(MedicalCategory.NameEn))
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount)
        );

        return new PagedResultDto<MedicalCategoryDto>(
            count,
            items.Select(i => _alumniMappers.MapToDto(i)).ToList()
        );
    }
}
