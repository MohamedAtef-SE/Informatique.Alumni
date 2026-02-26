using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Career;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.JobModerate)]
public class JobAdminAppService : AlumniAppService, IJobAdminAppService
{
    private readonly IRepository<Job, Guid> _jobRepository;
    private readonly IRepository<JobApplication, Guid> _applicationRepository;
    private readonly IIdentityUserRepository _userRepository;

    public JobAdminAppService(
        IRepository<Job, Guid> jobRepository,
        IRepository<JobApplication, Guid> applicationRepository,
        IIdentityUserRepository userRepository)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResultDto<JobAdminDto>> GetListAsync(JobAdminGetListInput input)
    {
        var queryable = await _jobRepository.GetQueryableAsync();

        if (input.IsActive.HasValue)
        {
            queryable = queryable.Where(x => x.IsActive == input.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x =>
                x.Title.Contains(input.Filter) ||
                x.Description.Contains(input.Filter));
        }

        var totalCount = queryable.Count();

        queryable = queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var jobs = queryable.ToList();

        // Get application counts (fixing N+1 issue)
        var jobIds = jobs.Select(j => j.Id).ToList();
        var appCounts = await AsyncExecuter.ToListAsync(
            (await _applicationRepository.GetQueryableAsync())
            .Where(a => jobIds.Contains(a.JobId))
            .GroupBy(a => a.JobId)
            .Select(g => new { JobId = g.Key, Count = g.Count() })
        );

        var items = jobs.Select(j => new JobAdminDto
        {
            Id = j.Id,
            Title = j.Title,
            Description = j.Description,
            Requirements = j.Requirements,
            CompanyId = j.CompanyId,
            IsActive = j.IsActive,
            ClosingDate = j.ClosingDate,
            CreationTime = j.CreationTime,
            ApplicationCount = appCounts.FirstOrDefault(c => c.JobId == j.Id)?.Count ?? 0
        }).ToList();

        return new PagedResultDto<JobAdminDto>(totalCount, items);
    }

    public async Task<JobAdminDto> GetAsync(Guid id)
    {
        var job = await _jobRepository.GetAsync(id);
        var appQueryable = await _applicationRepository.GetQueryableAsync();

        return new JobAdminDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Requirements = job.Requirements,
            CompanyId = job.CompanyId,
            IsActive = job.IsActive,
            ClosingDate = job.ClosingDate,
            CreationTime = job.CreationTime,
            ApplicationCount = appQueryable.Count(a => a.JobId == job.Id)
        };
    }

    public async Task ApproveJobAsync(Guid id)
    {
        var job = await _jobRepository.GetAsync(id);
        job.Reopen();
        await _jobRepository.UpdateAsync(job);
    }

    public async Task RejectJobAsync(Guid id)
    {
        var job = await _jobRepository.GetAsync(id);
        job.Close();
        await _jobRepository.UpdateAsync(job);
    }

    public async Task<PagedResultDto<JobApplicationAdminDto>> GetApplicationsAsync(
        Guid jobId, PagedAndSortedResultRequestDto input)
    {
        var queryable = (await _applicationRepository.GetQueryableAsync())
            .Where(x => x.JobId == jobId);

        var totalCount = queryable.Count();

        queryable = queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var applications = queryable.ToList();

        // Batch lookup user names via alumni profiles
        var alumniIds = applications.Select(a => a.AlumniId).ToList();

        var items = applications.Select(a => new JobApplicationAdminDto
        {
            Id = a.Id,
            JobId = a.JobId,
            AlumniId = a.AlumniId,
            AlumniName = "—", // Resolved in the frontend or by enrichment
            CvSnapshotBlobName = a.CvSnapshotBlobName,
            CreationTime = a.CreationTime
        }).ToList();

        return new PagedResultDto<JobApplicationAdminDto>(totalCount, items);
    }
}
