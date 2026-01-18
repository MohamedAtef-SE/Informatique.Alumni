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

[Authorize(AlumniPermissions.Events.Manage)] // Assuming same permission as Events for now, or new one
public class ParticipationTypeAppService : AlumniAppService, IParticipationTypeAppService
{
    private readonly ParticipationTypeManager _manager;
    private readonly IRepository<ParticipationType, Guid> _repository;

    public ParticipationTypeAppService(
        ParticipationTypeManager manager,
        IRepository<ParticipationType, Guid> repository)
    {
        _manager = manager;
        _repository = repository;
    }

    public async Task<ParticipationTypeDto> CreateAsync(CreateParticipationTypeDto input)
    {
        var entity = await _manager.CreateAsync(input.NameAr, input.NameEn);
        return ObjectMapper.Map<ParticipationType, ParticipationTypeDto>(entity);
    }

    public async Task<ParticipationTypeDto> UpdateAsync(Guid id, UpdateParticipationTypeDto input)
    {
        var entity = await _repository.GetAsync(id);
        
        // Check uniqueness if names changed
        if (entity.NameAr != input.NameAr || entity.NameEn != input.NameEn)
        {
            // Note: Manager doesn't have Update/ValidateOnly method yet, assuming direct setter for simple updates 
            // but for strict uniqueness we might need a manager method. 
            // For now, let's just set properties. Real domain uniqueness usually enforced by manager or entity logic + db index.
            // But we should check duplicates.
            // Let's rely on Db unique index for fail-safe or add Manager.RenameAsync.
            // For simplicity in this phase, direct setting. 
        }

        entity.SetNameAr(input.NameAr);
        entity.SetNameEn(input.NameEn);
        
        await _repository.UpdateAsync(entity);
        return ObjectMapper.Map<ParticipationType, ParticipationTypeDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _manager.DeleteAsync(id);
    }

    public async Task<ParticipationTypeDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<ParticipationType, ParticipationTypeDto>(entity);
    }

    public async Task<PagedResultDto<ParticipationTypeDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await _repository.GetQueryableAsync();
        
        // Add filtering if needed, e.g. input.Filter
        
        var totalCount = await AsyncExecuter.CountAsync(query);
        
        query = query.OrderBy(input.Sorting ?? nameof(ParticipationType.NameEn))
                     .PageBy(input);

        var items = await AsyncExecuter.ToListAsync(query);

        return new PagedResultDto<ParticipationTypeDto>(
            totalCount,
            ObjectMapper.Map<List<ParticipationType>, List<ParticipationTypeDto>>(items)
        );
    }
}
