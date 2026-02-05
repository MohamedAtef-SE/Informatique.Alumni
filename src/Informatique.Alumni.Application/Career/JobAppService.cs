using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
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
    private readonly IRepository<CurriculumVitae, Guid> _cvRepository;
    private readonly IBlobContainer<CvSnapshotBlobContainer> _blobContainer;
    private readonly AlumniApplicationMappers _alumniMappers;

    public JobAppService(
        IRepository<Job, Guid> jobRepository,
        IRepository<JobApplication, Guid> applicationRepository,
        IRepository<CurriculumVitae, Guid> cvRepository,
        IBlobContainer<CvSnapshotBlobContainer> blobContainer,
        AlumniApplicationMappers alumniMappers)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _cvRepository = cvRepository;
        _blobContainer = blobContainer;
        _alumniMappers = alumniMappers;
    }

    public async Task<PagedResultDto<JobDto>> GetJobsAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await _jobRepository.WithDetailsAsync();
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

    [Authorize(AlumniPermissions.Careers.JobApply)]
    public async Task ApplyAsync(Guid jobId)
    {
        var alumniId = CurrentUser.GetId();
        
        // Validate job exists
        var job = await _jobRepository.FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive);
        if (job == null)
        {
            throw new UserFriendlyException("Job not found or is no longer active.");
        }
        
        // Check duplicate application
        if (await _applicationRepository.AnyAsync(x => x.JobId == jobId && x.AlumniId == alumniId))
        {
            throw new UserFriendlyException("You have already applied for this job.");
        }

        // Validate CV existence (direct repository check - avoids AppService-to-AppService call)
        var hasCv = await _cvRepository.AnyAsync(x => x.AlumniId == alumniId);
        if (!hasCv)
        {
             throw new UserFriendlyException("You must create a CV before applying.", "Career:NoCvFound");
        }

        // TODO: Re-enable CV snapshot generation when PDF generator is configured
        // Currently skipping PDF generation to avoid dependency issues
        var blobName = $"placeholder_{jobId}_{alumniId}.pdf";

        // Create Application record with generated ID
        var application = new JobApplication(GuidGenerator.Create(), jobId, alumniId, blobName);
        await _applicationRepository.InsertAsync(application);
        
        Logger.LogInformation("Job application created successfully. JobId: {JobId}, AlumniId: {AlumniId}", jobId, alumniId);
    }

    [Authorize(AlumniPermissions.Careers.JobManage)]
    public async Task<List<JobApplicationDto>> GetApplicationsAsync(Guid jobId)
    {
        var query = await _applicationRepository.WithDetailsAsync();
        var list = await AsyncExecuter.ToListAsync(query.Where(x => x.JobId == jobId));
        return _alumniMappers.MapToDtos(list);
    }
}

[BlobContainerName("cv-snapshots")]
public class CvSnapshotBlobContainer { }
