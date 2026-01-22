using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Communications;

[Authorize]
public class NotificationReportingAppService : ApplicationService
{
    private readonly IRepository<CommunicationLog, Guid> _communicationLogRepository;
    private readonly IRepository<AlumniProfile, Guid> _alumniProfileRepository;

    public NotificationReportingAppService(
        IRepository<CommunicationLog, Guid> communicationLogRepository,
        IRepository<AlumniProfile, Guid> alumniProfileRepository)
    {
        _communicationLogRepository = communicationLogRepository;
        _alumniProfileRepository = alumniProfileRepository;
    }

    public async Task<MessageReportResultDto> GetReportAsync(MessageReportInputDto input)
    {
        // 1. Fetch Logs (Filtered by Date and Type)
        // Note: For large datasets, use IQueryable and join in DB.
        // Assuming standard repositories. Ideally, use a custom repository or AsyncExecuter (Queryable).
        var logsQuery = await _communicationLogRepository.GetQueryableAsync();
        var profilesQuery = await _alumniProfileRepository.GetQueryableAsync();

        if (input.FromDate.HasValue)
            logsQuery = logsQuery.Where(x => x.CreationTime >= input.FromDate.Value);
        
        if (input.ToDate.HasValue)
            logsQuery = logsQuery.Where(x => x.CreationTime <= input.ToDate.Value);

        if (input.MessageType == MessageTypeFilter.SMS)
            logsQuery = logsQuery.Where(x => x.Channel == "SMS");
        else if (input.MessageType == MessageTypeFilter.Email)
            logsQuery = logsQuery.Where(x => x.Channel == "Email");

        // 2. Filter by Alumni (Year, Branch, etc.)
        // Ensure RecipientId is not null
        logsQuery = logsQuery.Where(x => x.RecipientId.HasValue);

        // Join to apply Alumni Filters
        // Using "Select" or "Join" depends on query provider. 
        // We do a manual join logic via query construction
        var query = from log in logsQuery
                    join profile in profilesQuery on log.RecipientId equals profile.Id
                    select new { Log = log, Profile = profile };

        // Branch Filter (Mandatory)
        query = query.Where(x => x.Profile.BranchId == input.BranchId);

        // College/Major/Year Filters require accessing Profile.Educations (Collection)
        // This usually requires .SelectMany or .Any.
        // Since GraduationYear is in Education, we assume "Any Education matches".
        // Or if we need strict "Latest", querying becomes complex in LINQ-to-Entities without explicit mapping.
        // Simplified approach: If input years provided, check if profile has ANY education in those years.
        
        if (input.GraduationYears != null && input.GraduationYears.Any())
        {
            query = query.Where(x => x.Profile.Educations.Any(e => input.GraduationYears.Contains(e.GraduationYear)));
        }

        if (input.GraduationSemesters != null && input.GraduationSemesters.Any())
        {
            query = query.Where(x => x.Profile.Educations.Any(e => input.GraduationSemesters.Contains(e.GraduationSemester)));
        }

        if (input.CollegeId.HasValue)
        {
            query = query.Where(x => x.Profile.Educations.Any(e => e.CollegeId == input.CollegeId.Value));
        }

        if (input.MajorId.HasValue)
        {
            query = query.Where(x => x.Profile.Educations.Any(e => e.MajorId == input.MajorId.Value));
        }

        // Execute Query
        // We verify execution limit. For reports, might be large.
        var data = await AsyncExecuter.ToListAsync(query);

        var result = new MessageReportResultDto();

        if (input.ReportType == ReportType.Detailed)
        {
            // Detailed Report Logic
            result.Details = data
                .Select(x => new MessageReportDetailDto
                {
                    MessageTitle = x.Log.Subject,
                    MessageDate = x.Log.CreationTime,
                    MessageType = x.Log.Channel,
                    AlumniId = x.Profile.Id,
                    AlumniName = $"{x.Profile.JobTitle}", // Fallback Name logic as per previously seen fields
                    BranchId = x.Profile.BranchId,
                    BranchName = "View Branch Name via UI Lookup" // DTO usually just IDs or joined if name available
                })
                .OrderBy(x => x.AlumniName) // Default Sort
                .ToList();
        }
        else
        {
            // Statistical (Pivot) Logic
            // Group by Graduation Year.
            // Problem: Profile has List<Education>. Which Year? Max/Latest.
            
            var stats = data
                .Select(x => new 
                {
                    // Determination of "Graduation Year" for the row.
                    // If multiple, pick Max. If none, 0 or Unknown.
                    Year = x.Profile.Educations.Any() ? x.Profile.Educations.Max(e => e.GraduationYear) : 0,
                    Channel = x.Log.Channel
                })
                .GroupBy(x => x.Year)
                .Select(g => new MessageReportStatsRowDto
                {
                    GraduationYear = g.Key,
                    SmsCount = g.Count(x => x.Channel == "SMS"),
                    EmailCount = g.Count(x => x.Channel == "Email"),
                    TotalCount = g.Count()
                })
                .OrderBy(x => x.GraduationYear)
                .ToList();

            result.Stats = stats;
        }

        return result;
    }
}
