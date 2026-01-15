using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Career;

public interface ICVAppService : IApplicationService
{
    Task<CurriculumVitaeDto> GetMyCvAsync();
    Task<CurriculumVitaeDto> UpdateCvAsync(CurriculumVitaeDto input);
    Task<byte[]> DownloadCvPdfAsync(Guid cvId);
}

public interface IJobAppService : IApplicationService
{
    Task<PagedResultDto<JobDto>> GetJobsAsync(PagedAndSortedResultRequestDto input);
    Task<JobDto> CreateJobAsync(JobDto input);
    Task ApplyAsync(Guid jobId);
    Task<List<JobApplicationDto>> GetApplicationsAsync(Guid jobId);
}

public interface ICVAuditAppService : IApplicationService
{
    Task<PagedResultDto<CurriculumVitaeDto>> GetPendingCvsAsync(PagedAndSortedResultRequestDto input);
    Task ApproveCvAsync(Guid cvId);
    Task RejectCvAsync(Guid cvId, string reason);
}
