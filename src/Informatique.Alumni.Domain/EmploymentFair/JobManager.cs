using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.EmploymentFair;

public class JobManager : DomainService
{
    private readonly IRepository<AvailableJob, Guid> _availableJobRepository;
    private readonly IRepository<JobApplication, Guid> _jobApplicationRepository;

    public JobManager(
        IRepository<AvailableJob, Guid> availableJobRepository,
        IRepository<JobApplication, Guid> jobApplicationRepository)
    {
        _availableJobRepository = availableJobRepository;
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task ApplyForJobAsync(Guid jobId, Guid alumniId, string cvFileUrl)
    {
        // 1. Validation: Job exists and is active
        var job = await _availableJobRepository.GetAsync(jobId); // Throws EntityNotFoundException if missing
        if (!job.IsActive)
        {
            throw new UserFriendlyException("This job is no longer active.");
        }

        // AvailableOpportunitiesCount check (Optional business rule, but good practice if count is tracked)
        // logic: if (job.AvailableOpportunitiesCount.HasValue && job.AvailableOpportunitiesCount <= 0) ... 
        // Prompt says "Graduate sees... Optional: Number of available opportunities". 
        // Doesn't explicitly say to block application if 0, but usually yes. 
        // However, sticking to strict rules: "Validation: Ensure the Job is still 'Open' (Active) and the deadline hasn't passed". 
        // job.IsActive covers 'Open'.

        // 2. Duplicate Prevention
        var alreadyApplied = await _jobApplicationRepository.AnyAsync(x => x.JobId == jobId && x.AlumniId == alumniId);
        if (alreadyApplied)
        {
            throw new UserFriendlyException("You have already applied for this job.");
        }

        // 3. Create Application
        // ApplicationDate should be consistent (Now).
        var application = new JobApplication(
            GuidGenerator.Create(),
            jobId,
            alumniId,
            cvFileUrl,
            DateTime.Now
        );

        await _jobApplicationRepository.InsertAsync(application);
    }
}
