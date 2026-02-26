using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Profiles;
using Informatique.Alumni.Career;
using Informatique.Alumni.Events;
using Informatique.Alumni.Guidance;
using Informatique.Alumni.Syndicates;
using Informatique.Alumni.Membership;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.Dashboard)]
public class AdminDashboardAppService : ApplicationService, IAdminDashboardAppService
{
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<Job, Guid> _jobRepository;
    private readonly IRepository<AssociationEvent, Guid> _eventRepository;
    private readonly IRepository<AdvisingRequest, Guid> _advisingRepository;
    private readonly IRepository<College, Guid> _collegeRepository;
    private readonly IRepository<Education, Guid> _educationRepository;
    private readonly IRepository<SyndicateSubscription, Guid> _syndicateRepository;
    private readonly IRepository<AssociationRequest, Guid> _requestRepository;
    private readonly IRepository<AlumniEventRegistration, Guid> _eventRegistrationRepository;
    private readonly IRepository<SubscriptionFee, Guid> _subscriptionFeeRepository;
    private readonly IRepository<JobApplication, Guid> _jobApplicationRepository;

    public AdminDashboardAppService(
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<Job, Guid> jobRepository,
        IRepository<AssociationEvent, Guid> eventRepository,
        IRepository<AdvisingRequest, Guid> advisingRepository,
        IRepository<College, Guid> collegeRepository,
        IRepository<Education, Guid> educationRepository,
        IRepository<SyndicateSubscription, Guid> syndicateRepository,
        IRepository<AssociationRequest, Guid> requestRepository,
        IRepository<AlumniEventRegistration, Guid> eventRegistrationRepository,
        IRepository<SubscriptionFee, Guid> subscriptionFeeRepository,
        IRepository<JobApplication, Guid> jobApplicationRepository)
    {
        _profileRepository = profileRepository;
        _jobRepository = jobRepository;
        _eventRepository = eventRepository;
        _advisingRepository = advisingRepository;
        _collegeRepository = collegeRepository;
        _educationRepository = educationRepository;
        _syndicateRepository = syndicateRepository;
        _requestRepository = requestRepository;
        _eventRegistrationRepository = eventRegistrationRepository;
        _subscriptionFeeRepository = subscriptionFeeRepository;
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<AdminDashboardOverviewDto> GetOverviewAsync()
    {
        var dto = new AdminDashboardOverviewDto();

        // 1. Basic Counts
        dto.TotalAlumni = (int)await _profileRepository.GetCountAsync();
        dto.ActiveAlumni = await _profileRepository.CountAsync(p => p.Status == AlumniStatus.Active);
        dto.RejectedAlumni = await _profileRepository.CountAsync(p => p.Status == AlumniStatus.Rejected);
        dto.BannedAlumni = await _profileRepository.CountAsync(p => p.Status == AlumniStatus.Banned);
        dto.ActiveJobs = await _jobRepository.CountAsync(j => j.IsActive && j.ClosingDate > DateTime.UtcNow);
        dto.UpcomingEvents = await _eventRepository.CountAsync(e => e.Timeslots.Any(t => t.StartTime > DateTime.UtcNow));
        
        dto.PendingAlumni = await _requestRepository.CountAsync(r => r.Status == MembershipRequestStatus.Pending);
        dto.PendingGuidanceRequests = await _advisingRepository.CountAsync(r => r.Status == AdvisingRequestStatus.Pending);
        dto.PendingSyndicateRequests = await _syndicateRepository.CountAsync(r => r.Status == SyndicateStatus.Pending);

        // 2. Registration Trends (Last 6 Months)
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
        // Optimize: Filter in DB
        var profileQuery = await _profileRepository.GetQueryableAsync();
        var trendsData = profileQuery
            .Where(p => p.CreationTime >= sixMonthsAgo)
            .Select(p => p.CreationTime)
            .ToList();
            
        var trends = trendsData
            .GroupBy(d => new { d.Year, d.Month })
            .Select(g => new MonthlyMetricDto
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                Count = g.Count()
            })
            .ToList();

        for (int i = 0; i < 6; i++)
        {
            var date = DateTime.UtcNow.AddMonths(-i);
            var monthName = date.ToString("MMM");
            if (!trends.Any(t => t.Month == monthName))
            {
                trends.Add(new MonthlyMetricDto { Month = monthName, Count = 0 });
            }
        }
        dto.MonthlyRegistrations = trends.OrderByDescending(t => DateTime.ParseExact(t.Month, "MMM", null).Month).Reverse().ToList();

        // 3. College Distribution
        // Fix: Count unique alumni per college (taking primary/latest college if multiple) to ensure Sum <= Total
        var eduQuery = await _educationRepository.GetQueryableAsync();
        
        var alumniColleges = eduQuery
            .Where(e => e.CollegeId.HasValue)
            .Select(e => new { e.AlumniProfileId, e.CollegeId })
            .Distinct()
            .ToList();

        // If an alumni has multiple colleges, we need a rule. For a Pie Chart, strictly 1:1 is best.
        // We take the first one found (or we could order by graduation year if available in query).
        var uniqueAlumniColleges = alumniColleges
            .GroupBy(x => x.AlumniProfileId)
            .Select(g => g.First().CollegeId)
            .ToList();

        var collegeCounts = uniqueAlumniColleges
            .GroupBy(id => id)
            .Select(g => new { CollegeId = g.Key, Count = g.Count() })
            .ToList();

        var colleges = await _collegeRepository.GetListAsync();
        var collegeDict = colleges.ToDictionary(c => c.Id, c => c.Name);

        dto.AlumniByCollege = collegeCounts
            .Select(c => new DistributionItemDto
            {
                Label = (c.CollegeId.HasValue && collegeDict.ContainsKey(c.CollegeId.Value)) ? collegeDict[c.CollegeId.Value] : "Unknown",
                Count = c.Count
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        // 4. Total Revenue
        // Optimize: Sum in DB?
        // Logic requires joining with Fees for Requests. 
        // Requests: Sum (FeeAmount + DeliveryFee) where Approved/Paid
        // Events: Sum (PaidAmount) where Registered/Attended
        
        // Event Revenue
        var regQuery = await _eventRegistrationRepository.GetQueryableAsync();
        var eventRevenue = regQuery
            .Where(r => (r.Status == RegistrationStatus.Registered || r.Status == RegistrationStatus.Attended) && r.PaidAmount > 0)
            .Sum(r => r.PaidAmount) ?? 0;

        // Membership Revenue
        // We need to join with SubscriptionFee to get the amount, or if we trust the request snapshot if it existed. 
        // AssociationRequest doesn't seem to store the Fee Amount snapshot directly, it links to SubscriptionFeeId.
        // We will fetch valid requests and their fee IDs, then calculate.
        // To avoid N+1, we fetch the fee lookup once (which is small).
        
        var reqQuery = await _requestRepository.GetQueryableAsync();
        var validRequests = reqQuery
            .Where(r => r.Status == MembershipRequestStatus.Approved || r.Status == MembershipRequestStatus.Paid)
            .Select(r => new { r.SubscriptionFeeId, r.DeliveryFee })
            .ToList();

        var subscriptionFees = (await _subscriptionFeeRepository.GetListAsync())
            .ToDictionary(x => x.Id, x => x.Amount);

        decimal membershipRevenue = validRequests.Sum(r => 
            (subscriptionFees.ContainsKey(r.SubscriptionFeeId) ? subscriptionFees[r.SubscriptionFeeId] : 0) + r.DeliveryFee);

        dto.TotalRevenue = membershipRevenue + eventRevenue;

        // 5. Top Employers
        // Optimize: Group in DB
        // Note: EF Core GroupBy translation support depends on provider. SQL Server supports it.
        var profilesQuery = await _profileRepository.GetQueryableAsync();
        
        var topEmployers = profilesQuery
            .Where(p => p.Company != null && p.Company != "")
            .GroupBy(p => p.Company)
            .Select(g => new { Company = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        dto.TopEmployers = topEmployers
            .Select(x => new DistributionItemDto { Label = x.Company, Count = x.Count })
            .ToList();

        // 6. Top Locations
        // Optimize: Group in DB. 
        // Grouping by concatenated string $"City, Country" might not translate well to SQL in all versions.
        // Safer: Group by City AND Country.
        var topLocations = profilesQuery
            .Where(p => p.City != null && p.City != "" && p.Country != null && p.Country != "")
            .GroupBy(p => new { p.City, p.Country })
            .Select(g => new { g.Key.City, g.Key.Country, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        dto.TopLocations = topLocations
            .Select(x => new DistributionItemDto { Label = $"{x.City}, {x.Country}", Count = x.Count })
            .ToList();

        // 7. Recent Activity Feed
        var recentActivities = new List<ActivityItemDto>();

        // Top 5 New Profiles
        var recentProfiles = profilesQuery
            .OrderByDescending(p => p.CreationTime)
            .Take(5)
            .Select(p => new { p.CreationTime })
            .ToList();
            
        recentActivities.AddRange(recentProfiles.Select(p => new ActivityItemDto 
        { 
            Time = p.CreationTime, 
            Type = "User", 
            Description = "New alumni registration" 
        }));

        // Top 5 New Registrations
        var recentRegs = regQuery
            .OrderByDescending(r => r.CreationTime)
            .Take(5)
            .Select(r => new { r.CreationTime, r.EventId })
            .ToList();
            
        if (recentRegs.Any())
        {
            var eventIds = recentRegs.Select(r => r.EventId).Distinct().ToList();
            var eventNames = (await _eventRepository.GetQueryableAsync())
                .Where(e => eventIds.Contains(e.Id))
                .Select(e => new { e.Id, e.NameEn })
                .ToDictionary(e => e.Id, e => e.NameEn);
                
            recentActivities.AddRange(recentRegs.Select(r => new ActivityItemDto
            {
                Time = r.CreationTime,
                Type = "Event",
                Description = eventNames.ContainsKey(r.EventId) ? $"Registration for {eventNames[r.EventId]}" : "Event registration"
            }));
        }

        // Top 5 Job Applications
        var jobAppsQuery = await _jobApplicationRepository.GetQueryableAsync();
        var recentJobApps = jobAppsQuery
            .OrderByDescending(a => a.CreationTime)
            .Take(5)
            .Select(a => new { a.CreationTime, a.JobId })
            .ToList();

        if (recentJobApps.Any())
        {
             var jobIds = recentJobApps.Select(a => a.JobId).Distinct().ToList();
             var jobTitles = (await _jobRepository.GetQueryableAsync())
                 .Where(j => jobIds.Contains(j.Id))
                 .Select(j => new { j.Id, j.Title })
                 .ToDictionary(j => j.Id, j => j.Title);

             recentActivities.AddRange(recentJobApps.Select(a => new ActivityItemDto
             {
                 Time = a.CreationTime,
                 Type = "Job",
                 Description = jobTitles.ContainsKey(a.JobId) ? $"Applied to {jobTitles[a.JobId]}" : "Job application"
             }));
        }

        dto.RecentActivities = recentActivities
            .OrderByDescending(a => a.Time)
            .Take(5)
            .ToList();

        return dto;
    }
}
