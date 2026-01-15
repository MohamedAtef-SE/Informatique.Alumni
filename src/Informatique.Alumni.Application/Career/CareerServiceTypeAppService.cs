using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Informatique.Alumni.Permissions;
using System.Linq.Dynamic.Core;

namespace Informatique.Alumni.Career;

[Authorize(AlumniPermissions.Career.Manage)]
public class CareerServiceTypeAppService : ApplicationService, ICareerServiceTypeAppService
{
    private readonly IRepository<CareerServiceType, Guid> _repository;
    private readonly CareerServiceTypeManager _manager;
    private readonly AlumniApplicationMappers _mappers;

    public CareerServiceTypeAppService(
        IRepository<CareerServiceType, Guid> repository,
        CareerServiceTypeManager manager,
        AlumniApplicationMappers mappers)
    {
        _repository = repository;
        _manager = manager;
        _mappers = mappers;
    }

    public async Task<CareerServiceTypeDto> CreateAsync(CreateUpdateCareerServiceTypeDto input)
    {
        var entity = await _manager.CreateAsync(input.NameAr, input.NameEn);
        if (input.IsActive) entity.Activate();
        else entity.Deactivate();

        await _repository.InsertAsync(entity);
        return _mappers.MapToDto(entity);
    }

    public async Task<CareerServiceTypeDto> UpdateAsync(Guid id, CreateUpdateCareerServiceTypeDto input)
    {
        var entity = await _repository.GetAsync(id);
        await _manager.UpdateAsync(entity, input.NameAr, input.NameEn, input.IsActive);
        await _repository.UpdateAsync(entity);
        return _mappers.MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _manager.DeleteAsync(id);
    }

    public async Task<CareerServiceTypeDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return _mappers.MapToDto(entity);
    }

    public async Task<PagedResultDto<CareerServiceTypeDto>> GetListAsync(CareerServiceTypeFilterDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (input.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == input.IsActive);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);
        
        query = query.OrderBy(input.Sorting ?? "CreationTime desc")
                     .PageBy(input.SkipCount, input.MaxResultCount);

        var items = await AsyncExecuter.ToListAsync(query);

        return new PagedResultDto<CareerServiceTypeDto>(
            totalCount,
            _mappers.MapToDtos(items)
        );
    }
}
