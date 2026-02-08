using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Benefits;

[Authorize]
public class AcademicGrantAppService : AlumniAppService, IAcademicGrantAppService
{
    private readonly IRepository<AcademicGrant, Guid> _grantRepository;
    private readonly AcademicGrantManager _grantManager;
    private readonly AlumniApplicationMappers _alumniMappers;

    public AcademicGrantAppService(
        IRepository<AcademicGrant, Guid> grantRepository,
        AcademicGrantManager grantManager,
        AlumniApplicationMappers alumniMappers)
    {
        _grantRepository = grantRepository;
        _grantManager = grantManager;
        _alumniMappers = alumniMappers;
    }

    [Authorize(AlumniPermissions.Benefits.View)]
    public async Task<PagedResultDto<AcademicGrantDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var count = await _grantRepository.GetCountAsync();
        var query = await _grantRepository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(input.Sorting ?? "CreationTime desc").PageBy(input.SkipCount, input.MaxResultCount));
        
        return new PagedResultDto<AcademicGrantDto>(count, _alumniMappers.MapToDtos(list));
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task<AcademicGrantDto> CreateAsync(CreateUpdateAcademicGrantDto input)
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
    public async Task<AcademicGrantDto> UpdateAsync(Guid id, CreateUpdateAcademicGrantDto input)
    {
        var grant = await _grantRepository.GetAsync(id);
        
        grant.SetName(input.NameAr, input.NameEn);
        grant.SetType(input.Type);
        grant.SetPercentage(input.Percentage);
        
        await _grantRepository.UpdateAsync(grant);
        return _alumniMappers.MapToDto(grant);
    }

    [Authorize(AlumniPermissions.Benefits.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _grantRepository.DeleteAsync(id);
    }
}
