using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Informatique.Alumni.Permissions;

namespace Informatique.Alumni.Branches;

[Authorize(AlumniPermissions.Branches.Default)]
public class BranchAppService : 
    CrudAppService<Branch, BranchDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateBranchDto>,
    IBranchAppService
{
    protected override string? GetListPolicyName { get; set; } = AlumniPermissions.Branches.Default;
    protected override string? GetPolicyName { get; set; } = AlumniPermissions.Branches.Default;
    protected override string? CreatePolicyName { get; set; } = AlumniPermissions.Branches.Create;
    protected override string? UpdatePolicyName { get; set; } = AlumniPermissions.Branches.Edit;
    protected override string? DeletePolicyName { get; set; } = AlumniPermissions.Branches.Delete;

    private readonly IDistributedCache<List<BranchDto>> _academicStructureCache;

    public BranchAppService(
        IRepository<Branch, Guid> repository,
        IDistributedCache<List<BranchDto>> academicStructureCache) 
        : base(repository)
    {
        _academicStructureCache = academicStructureCache;
    }

    public async Task<List<BranchDto>> GetAcademicStructureAsync()
    {
        return await _academicStructureCache.GetOrAddAsync(
            "AcademicStructure",
            async () =>
            {
                var branches = await Repository.GetListAsync();
                return ObjectMapper.Map<List<Branch>, List<BranchDto>>(branches);
            },
            () => new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            }
        );
    }
}
