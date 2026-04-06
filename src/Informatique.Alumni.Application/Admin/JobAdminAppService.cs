using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Career;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.BlobStoring;
using Informatique.Alumni.Profiles;
using Volo.Abp.Content;
using Volo.Abp;
using System.Collections.Generic;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.JobModerate)]
public class JobAdminAppService : AlumniAppService, IJobAdminAppService
{
    private readonly IRepository<Job, Guid> _jobRepository;
    private readonly IRepository<JobApplication, Guid> _applicationRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<CurriculumVitae, Guid> _cvRepository;
    private readonly IBlobContainer<CvSnapshotBlobContainer> _blobContainer;

    public JobAdminAppService(
        IRepository<Job, Guid> jobRepository,
        IRepository<JobApplication, Guid> applicationRepository,
        IIdentityUserRepository userRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<CurriculumVitae, Guid> cvRepository,
        IBlobContainer<CvSnapshotBlobContainer> blobContainer)
    {
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _cvRepository = cvRepository;
        _blobContainer = blobContainer;
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

        // Batch lookup alumni profiles to get UserIds
        var alumniIds = applications.Select(a => a.AlumniId).Distinct().ToList();
        var profiles = await _profileRepository.GetListAsync(x => alumniIds.Contains(x.Id));
        
        // Batch lookup Identity Users to get Names
        var userIds = profiles.Select(p => p.UserId).Distinct().ToList();
        var users = (await _userRepository.GetListAsync()).Where(u => userIds.Contains(u.Id)).ToList();
        var userDict = users.ToDictionary(u => u.Id, u => $"{u.Name} {u.Surname}");
        var profileToUserDict = profiles.ToDictionary(p => p.Id, p => p.UserId);

        var items = applications.Select(a => new JobApplicationAdminDto
        {
            Id = a.Id,
            JobId = a.JobId,
            AlumniId = a.AlumniId,
            AlumniName = profileToUserDict.TryGetValue(a.AlumniId, out var uId) && userDict.TryGetValue(uId, out var name) ? name : "Unknown Alumni",
            CvSnapshotBlobName = a.CvSnapshotBlobName,
            CreationTime = a.CreationTime
        }).ToList();

        return new PagedResultDto<JobApplicationAdminDto>(totalCount, items);
    }

    public async Task<AlumniCvDto> GetAlumniCvAsync(Guid alumniId)
    {
        var profile = await _profileRepository.GetAsync(alumniId);
        var user = await _userRepository.GetAsync(profile.UserId);
        var cvQuery = await _cvRepository.WithDetailsAsync(
            x => x.Experiences, 
            x => x.Educations, 
            x => x.Skills,
            x => x.Languages,
            x => x.Certifications,
            x => x.Projects,
            x => x.SocialLinks);
            
        var alumniCv = await AsyncExecuter.FirstOrDefaultAsync(cvQuery.Where(x => x.AlumniId == alumniId));
        
        if (alumniCv == null)
        {
            throw new UserFriendlyException("Curriculum Vitae not found for this alumni.");
        }

        return new AlumniCvDto
        {
            AlumniId = alumniId,
            FullName = $"{user.Name} {user.Surname}",
            Summary = alumniCv.Summary,
            Experiences = alumniCv.Experiences.Select(x => new CvExperienceDto
            {
                Company = x.Company,
                Position = x.Position,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Description = x.Description
            }).ToList(),
            Educations = alumniCv.Educations.Select(x => new CvEducationDto
            {
                Institution = x.Institution,
                Degree = x.Degree,
                StartDate = x.StartDate,
                EndDate = x.EndDate
            }).ToList(),
            Skills = alumniCv.Skills.Select(x => new CvSkillDto
            {
                Name = x.Name,
                ProficiencyLevel = x.ProficiencyLevel
            }).ToList(),
            Languages = alumniCv.Languages.Select(x => new CvLanguageDto
            {
                Name = x.Name,
                FluencyLevel = x.FluencyLevel
            }).ToList(),
            Certifications = alumniCv.Certifications.Select(x => new CvCertificationDto
            {
                Name = x.Name,
                Issuer = x.Issuer,
                Date = x.Date
            }).ToList(),
            Projects = alumniCv.Projects.Select(x => new CvProjectDto
            {
                Name = x.Name,
                Description = x.Description,
                Link = x.Link
            }).ToList(),
            SocialLinks = alumniCv.SocialLinks.Select(x => new CvSocialLinkDto
            {
                Platform = x.Platform,
                Url = x.Url
            }).ToList()
        };
    }

    public async Task<IRemoteStreamContent> GetApplicationCvAsync(Guid id)
    {
        var application = await _applicationRepository.GetAsync(id);
        if (string.IsNullOrWhiteSpace(application.CvSnapshotBlobName))
        {
            throw new UserFriendlyException("CV snapshot not found for this application.");
        }

        // Safety check to prevent 500 if blob is missing from storage
        if (!await _blobContainer.ExistsAsync(application.CvSnapshotBlobName))
        {
             throw new UserFriendlyException("The archived CV PDF could not be found in storage. Please view the live profile instead.");
        }

        var stream = await _blobContainer.GetAsync(application.CvSnapshotBlobName);
        return new RemoteStreamContent(stream, application.CvSnapshotBlobName, "application/pdf");
    }
}
