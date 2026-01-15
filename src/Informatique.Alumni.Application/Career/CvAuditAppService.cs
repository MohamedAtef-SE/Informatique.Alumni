using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Career;

[Authorize(AlumniPermissions.Careers.CvAudit)]
public class CvAuditAppService : AlumniAppService, ICVAuditAppService
{
    private readonly IRepository<CurriculumVitae, Guid> _cvRepository;
    private readonly CvManager _cvManager;
    private readonly AlumniApplicationMappers _alumniMappers;

    public CvAuditAppService(
        IRepository<CurriculumVitae, Guid> cvRepository,
        CvManager cvManager,
        AlumniApplicationMappers alumniMappers)
    {
        _cvRepository = cvRepository;
        _cvManager = cvManager;
        _alumniMappers = alumniMappers;
    }

    public async Task<PagedResultDto<CurriculumVitaeDto>> GetPendingCvsAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await _cvRepository.GetQueryableAsync();
        query = query.Where(x => x.Status == CvStatus.PendingAudit || x.Status == CvStatus.Draft); // Showing draft for evaluation
        
        var count = await AsyncExecuter.CountAsync(query);
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(input.Sorting ?? "LastModificationTime desc").PageBy(input.SkipCount, input.MaxResultCount));
        
        return new PagedResultDto<CurriculumVitaeDto>(count, _alumniMappers.MapToDtos(list));
    }

    public async Task ApproveCvAsync(Guid cvId)
    {
        var cv = await _cvRepository.GetAsync(cvId);
        _cvManager.Approve(cv);
        await _cvRepository.UpdateAsync(cv);
    }

    public async Task RejectCvAsync(Guid cvId, string reason)
    {
        var cv = await _cvRepository.GetAsync(cvId);
        _cvManager.Reject(cv, reason);
        await _cvRepository.UpdateAsync(cv);
    }
}
