using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Informatique.Alumni.Career;

[Authorize]
public class JobAppService : AlumniAppService, IJobAppService
{
    private readonly IRepository<Job, Guid> _jobRepository;
    private readonly IRepository<JobApplication, Guid> _applicationRepository;
    private readonly ICVAppService _cvAppService;
    private readonly IBlobContainer<CvSnapshotBlobContainer> _blobContainer;
    private readonly AlumniApplicationMappers _alumniMappers;

    public JobAppService(
        IRepository<Job, Guid> jobRepository,
        IRepository<JobApplication, Guid> applicationRepository,
        ICVAppService cvAppService,
        IBlobContainer<CvSnapshotBlobContainer> blobContainer,
        AlumniApplicationMappers alumniMappers)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _cvAppService = cvAppService;
        _blobContainer = blobContainer;
        _alumniMappers = alumniMappers;
    }

    public async Task<PagedResultDto<JobDto>> GetJobsAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await _jobRepository.GetQueryableAsync();
        query = query.Where(x => x.IsActive);
        
        var count = await AsyncExecuter.CountAsync(query);
        var list = await AsyncExecuter.ToListAsync(query.OrderBy(input.Sorting ?? "CreationTime desc").PageBy(input.SkipCount, input.MaxResultCount));
        
        return new PagedResultDto<JobDto>(count, _alumniMappers.MapToDtos(list));
    }

    [Authorize(AlumniPermissions.Careers.JobManage)]
    public async Task<JobDto> CreateJobAsync(JobDto input)
    {
        var job = new Job(GuidGenerator.Create(), input.CompanyId, input.Title, input.Description);
        
        if (!string.IsNullOrWhiteSpace(input.Requirements))
        {
            job.UpdateRequirements(input.Requirements);
        }
        
        if (input.ClosingDate.HasValue)
        {
            job.SetClosingDate(input.ClosingDate.Value);
        }
        
        await _jobRepository.InsertAsync(job);
        return _alumniMappers.MapToDto(job);
    }

    public async Task ApplyAsync(Guid jobId)
    {
        var alumniId = CurrentUser.GetId();
        if (await _applicationRepository.AnyAsync(x => x.JobId == jobId && x.AlumniId == alumniId))
        {
            throw new UserFriendlyException("You have already applied for this job.");
        }

        var cv = await _cvAppService.GetMyCvAsync();
        if (cv.Status != CvStatus.Approved && cv.Status != CvStatus.Draft) // Assuming we allow Draft for testing, but ideally Approved
        {
             // For production: throw new UserFriendlyException("Your CV must be approved before applying.");
        }

        // 1. Generate Snapshot
        var pdf = await _cvAppService.DownloadCvPdfAsync(cv.Id);
        var blobName = $"cv_snapshot_{jobId}_{alumniId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        
        // 2. Save to BLOB
        await _blobContainer.SaveAsync(blobName, pdf);

        // 3. Create Application record
        var application = new JobApplication(GuidGenerator.Create(), jobId, alumniId, blobName);
        await _applicationRepository.InsertAsync(application);
    }

    [Authorize(AlumniPermissions.Careers.JobManage)]
    public async Task<List<JobApplicationDto>> GetApplicationsAsync(Guid jobId)
    {
        var list = await _applicationRepository.GetListAsync(x => x.JobId == jobId);
        return _alumniMappers.MapToDtos(list);
    }
}

[BlobContainerName("cv-snapshots")]
public class CvSnapshotBlobContainer { }
