using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Informatique.Alumni.Permissions;

namespace Informatique.Alumni.Events;

[Authorize(AlumniPermissions.Events.Manage)]
public class ActivityTypeAppService : AlumniAppService, IActivityTypeAppService
{
    private readonly IRepository<ActivityType, Guid> _activityTypeRepository;
    private readonly ActivityTypeManager _activityTypeManager;

    public ActivityTypeAppService(
        IRepository<ActivityType, Guid> activityTypeRepository,
        ActivityTypeManager activityTypeManager)
    {
        _activityTypeRepository = activityTypeRepository;
        _activityTypeManager = activityTypeManager;
    }

    public async Task<PagedResultDto<ActivityTypeDto>> GetListAsync(ActivityTypeFilterDto input)
    {
        var query = await _activityTypeRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x => x.NameAr.Contains(input.Filter) || x.NameEn.Contains(input.Filter));
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == input.IsActive.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = query.OrderBy(input.Sorting ?? nameof(ActivityType.NameEn))
                     .Skip(input.SkipCount)
                     .Take(input.MaxResultCount);

        var items = await AsyncExecuter.ToListAsync(query);

        return new PagedResultDto<ActivityTypeDto>(
            totalCount,
            ObjectMapper.Map<List<ActivityType>, List<ActivityTypeDto>>(items)
        );
    }

    public async Task<ActivityTypeDto> GetAsync(Guid id)
    {
        var activityType = await _activityTypeRepository.GetAsync(id);
        return ObjectMapper.Map<ActivityType, ActivityTypeDto>(activityType);
    }

    public async Task<ActivityTypeDto> CreateAsync(CreateActivityTypeDto input)
    {
        var activityType = await _activityTypeManager.CreateAsync(
            input.NameAr,
            input.NameEn,
            input.IsActive
        );

        return ObjectMapper.Map<ActivityType, ActivityTypeDto>(activityType);
    }

    public async Task<ActivityTypeDto> UpdateAsync(Guid id, UpdateActivityTypeDto input)
    {
        var activityType = await _activityTypeRepository.GetAsync(id);

        var updated = await _activityTypeManager.UpdateAsync(
            activityType,
            input.NameAr,
            input.NameEn,
            input.IsActive
        );

        return ObjectMapper.Map<ActivityType, ActivityTypeDto>(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _activityTypeManager.DeleteAsync(id);
    }
}
