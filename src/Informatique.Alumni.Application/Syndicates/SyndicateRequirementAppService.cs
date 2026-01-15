
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Syndicates;

[Authorize]
public class SyndicateRequirementAppService : AlumniAppService, ISyndicateRequirementAppService
{
    private readonly IRepository<SyndicateRequirement, Guid> _repository;
    private readonly AlumniApplicationMappers _alumniMappers;

    public SyndicateRequirementAppService(
        IRepository<SyndicateRequirement, Guid> repository,
        AlumniApplicationMappers alumniMappers)
    {
        _repository = repository;
        _alumniMappers = alumniMappers;
    }

    public async Task<PagedResultDto<SyndicateRequirementDto>> GetListAsync(SyndicateRequirementFilterDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.CollegeName))
        {
            query = query.Where(x => x.CollegeName.Contains(input.CollegeName));
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        var dtos = _alumniMappers.MapToDtos(items);

        return new PagedResultDto<SyndicateRequirementDto>(totalCount, dtos);
    }
}
