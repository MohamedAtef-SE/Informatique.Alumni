using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Informatique.Alumni.Permissions;

namespace Informatique.Alumni.Guidance;

[Authorize]
public class AdvisoryCategoryAppService : AlumniAppService, IAdvisoryCategoryAppService
{
    private readonly IRepository<AdvisoryCategory, Guid> _repository;
    private readonly AlumniApplicationMappers _mappers;

    public AdvisoryCategoryAppService(
        IRepository<AdvisoryCategory, Guid> repository,
        AlumniApplicationMappers mappers)
    {
        _repository = repository;
        _mappers = mappers;
    }

    [Authorize(AlumniPermissions.Admin.GuidanceManage)]
    public async Task<AdvisoryCategoryDto> CreateAsync(CreateUpdateAdvisoryCategoryDto input)
    {
        var category = new AdvisoryCategory(
            GuidGenerator.Create(),
            input.NameAr,
            input.NameEn,
            input.IsActive
        );

        await _repository.InsertAsync(category);
        return _mappers.MapToDto(category);
    }

    [Authorize(AlumniPermissions.Admin.GuidanceManage)]
    public async Task<AdvisoryCategoryDto> UpdateAsync(Guid id, CreateUpdateAdvisoryCategoryDto input)
    {
        var category = await _repository.GetAsync(id);
        category.SetName(input.NameAr, input.NameEn);
        
        if (input.IsActive) category.Activate();
        else category.Deactivate();

        await _repository.UpdateAsync(category);
        return _mappers.MapToDto(category);
    }

    [Authorize(AlumniPermissions.Admin.GuidanceManage)]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    [Authorize(AlumniPermissions.Admin.GuidanceManage)]
    public async Task<AdvisoryCategoryDto> GetAsync(Guid id)
    {
        var category = await _repository.GetAsync(id);
        return _mappers.MapToDto(category);
    }

    [Authorize(AlumniPermissions.Admin.GuidanceManage)]
    public async Task<PagedResultDto<AdvisoryCategoryDto>> GetListAsync([FromQuery] AdvisoryCategoryFilterDto input)
    {
        var queryable = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x => 
                x.NameAr.Contains(input.Filter) || 
                x.NameEn.Contains(input.Filter));
        }

        if (input.IsActive.HasValue)
        {
            queryable = queryable.Where(x => x.IsActive == input.IsActive.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        
        queryable = queryable
            .OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? nameof(AdvisoryCategory.NameEn) : input.Sorting)
            .PageBy(input);

        var items = await AsyncExecuter.ToListAsync(queryable);
        
        return new PagedResultDto<AdvisoryCategoryDto>(
            totalCount,
            _mappers.MapToDtos(items)
        );
    }

    public async Task<List<AdvisoryCategoryDto>> GetActiveListAsync()
    {
        var items = await _repository.GetListAsync(x => x.IsActive);
        return _mappers.MapToDtos(items);
    }
}
