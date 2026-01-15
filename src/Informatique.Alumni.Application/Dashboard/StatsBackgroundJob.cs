using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Career;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Profiles;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace Informatique.Alumni.Dashboard;

public class StatsBackgroundJob : AsyncBackgroundJob<StatsArgs>, ITransientDependency
{
    private readonly IRepository<DailyStats, Guid> _statsRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<Job, Guid> _jobRepository;
    // Assuming a Payment/Revenue table exists from previous logic (Membership Transactions)
    private readonly IRepository<PaymentTransaction, Guid> _paymentRepository;

    public StatsBackgroundJob(
        IRepository<DailyStats, Guid> statsRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<Job, Guid> jobRepository,
        IRepository<PaymentTransaction, Guid> paymentRepository)
    {
        _statsRepository = statsRepository;
        _userRepository = userRepository;
        _profileRepository = profileRepository;
        _jobRepository = jobRepository;
        _paymentRepository = paymentRepository;
    }

    public override async Task ExecuteAsync(StatsArgs args)
    {
        var today = DateTime.Today;
        
        // Use a simple query for aggregate data
        var alumniCount = await _userRepository.CountAsync();
        
        var profileQuery = await _profileRepository.GetQueryableAsync();
        // Simple logic for employment rate: users with job titles vs total users
        var employedCount = profileQuery.Count(x => !string.IsNullOrEmpty(x.JobTitle));
        var employmentRate = alumniCount > 0 ? (double)employedCount / alumniCount * 100 : 0;
        
        var activeJobsCount = await _jobRepository.CountAsync(x => x.IsActive);
        
        var revenueSum = (await _paymentRepository.GetQueryableAsync())
            .Where(x => x.Status == PaymentStatus.Completed)
            .Sum(x => (decimal?)x.Amount) ?? 0;

        var stats = await _statsRepository.FirstOrDefaultAsync(x => x.Date == today);
        if (stats == null)
        {
            stats = new DailyStats(Guid.NewGuid(), today);
            stats.TotalAlumniCount = alumniCount;
            stats.EmploymentRate = employmentRate;
            stats.ActiveJobsCount = activeJobsCount;
            stats.TotalRevenue = revenueSum;
            await _statsRepository.InsertAsync(stats);
        }
        else
        {
            stats.TotalAlumniCount = alumniCount;
            stats.EmploymentRate = employmentRate;
            stats.ActiveJobsCount = activeJobsCount;
            stats.TotalRevenue = revenueSum;
            stats.LastUpdated = DateTime.Now;
            await _statsRepository.UpdateAsync(stats);
        }
    }
}

public class StatsArgs { }
