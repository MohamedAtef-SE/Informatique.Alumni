using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Directory;

[Authorize(AlumniPermissions.Directory.Search)]
public class DirectoryAppService : AlumniAppService, IDirectoryAppService
{
    private readonly IRepository<AlumniDirectoryCache, Guid> _cacheRepository;
    private readonly AlumniApplicationMappers _alumniMappers;

    public DirectoryAppService(
        IRepository<AlumniDirectoryCache, Guid> cacheRepository,
        AlumniApplicationMappers alumniMappers)
    {
        _cacheRepository = cacheRepository;
        _alumniMappers = alumniMappers;
    }

    public async Task<PagedResultDto<AlumniDirectoryDto>> SearchAsync(AlumniSearchRequestDto input)
    {
        if (input.Sorting.IsNullOrWhiteSpace())
        {
            input.Sorting = nameof(AlumniDirectoryCache.FullName);
        }

        var query = await _cacheRepository.GetQueryableAsync();
        
        // Privacy: Exclude those who opted out
        query = query.Where(x => x.ShowInDirectory);

        // Filters
        query = query
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => 
                x.FullName.Contains(input.Filter!) || 
                x.JobTitle.Contains(input.Filter!) || 
                x.Company.Contains(input.Filter!))
            .WhereIf(!input.Major.IsNullOrWhiteSpace(), x => x.Major == input.Major)
            .WhereIf(!input.College.IsNullOrWhiteSpace(), x => x.College == input.College)
            .WhereIf(input.GraduationYear.HasValue, x => x.GraduationYear == input.GraduationYear);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderBy(input.Sorting).PageBy(input.SkipCount, input.MaxResultCount)
        );

        return new PagedResultDto<AlumniDirectoryDto>(
            totalCount,
            _alumniMappers.MapToDtos(entities)
        );
    }
}
