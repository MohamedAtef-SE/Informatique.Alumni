using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Informatique.Alumni.Profiles;

namespace Informatique.Alumni.EmploymentFair;

[Authorize]
public class EmploymentReportingAppService : ApplicationService
{
    private readonly IRepository<CvSentHistory, Guid> _sentHistoryRepository;
    private readonly IRepository<CvReview, Guid> _cvReviewRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;

    public EmploymentReportingAppService(
        IRepository<CvSentHistory, Guid> sentHistoryRepository,
        IRepository<CvReview, Guid> cvReviewRepository,
        IRepository<AlumniProfile, Guid> profileRepository)
    {
        _sentHistoryRepository = sentHistoryRepository;
        _cvReviewRepository = cvReviewRepository;
        _profileRepository = profileRepository;
    }

    /// <summary>
    /// Pivot: Company vs Date (Aggregated by Company)
    /// </summary>
    public async Task<List<SentCvStatsDto>> GetSentCvStatsAsync()
    {
        var queryable = await _sentHistoryRepository.GetQueryableAsync();

        var stats = queryable
            .GroupBy(x => x.CompanyEmail)
            .Select(g => new SentCvStatsDto
            {
                CompanyEmail = g.Key,
                SentCount = g.Count(),
                LastSentDate = g.Max(x => x.SentDate)
            })
            .OrderByDescending(x => x.SentCount)
            .ToList();

        return stats;
    }

    /// <summary>
    /// Pivot: Graduation Year vs Status
    /// </summary>
    public async Task<List<CvAuditStatsDto>> GetAuditStatsAsync()
    {
        var reviewQuery = await _cvReviewRepository.GetQueryableAsync();
        var profileQuery = await _profileRepository.GetQueryableAsync();

        // Join to get Graduation Year
        // Note: GraduationYear is on Education Collection. We take the Max Year.
        var query = from review in reviewQuery
                    join profile in profileQuery on review.GraduateId equals profile.Id
                    select new 
                    {
                        review.Status,
                        GraduationYear = profile.Educations.Any() ? profile.Educations.Max(e => e.GraduationYear) : 0
                    };

        // Materialize for in-memory grouping (safest for complex projections involving Collections)
        var data = await AsyncExecuter.ToListAsync(query);

        var grouped = data
            .GroupBy(x => x.GraduationYear)
            .Select(g => new CvAuditStatsDto
            {
                GraduationYear = g.Key,
                PendingCount = g.Count(x => x.Status == CvReviewStatus.Pending),
                InProgressCount = g.Count(x => x.Status == CvReviewStatus.InProgress),
                CompletedCount = g.Count(x => x.Status == CvReviewStatus.Completed),
                TotalCount = g.Count()
            })
            .OrderByDescending(x => x.GraduationYear)
            .ToList();

        return grouped;
    }
}
