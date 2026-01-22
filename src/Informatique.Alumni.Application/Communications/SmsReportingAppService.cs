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
public class SmsReportingAppService : ApplicationService
{
    private readonly IRepository<CommunicationLog, Guid> _communicationLogRepository;
    private readonly IRepository<AlumniProfile, Guid> _alumniProfileRepository;

    public SmsReportingAppService(
        IRepository<CommunicationLog, Guid> communicationLogRepository,
        IRepository<AlumniProfile, Guid> alumniProfileRepository)
    {
        _communicationLogRepository = communicationLogRepository;
        _alumniProfileRepository = alumniProfileRepository;
    }

    public async Task<SmsReportResultDto> GetReportAsync(SmsReportInputDto input)
    {
        // 1. Fetch Queryables
        var logsQuery = await _communicationLogRepository.GetQueryableAsync();
        var profilesQuery = await _alumniProfileRepository.GetQueryableAsync();

        // 2. Filter Logs
        if (input.FromDate.HasValue)
            logsQuery = logsQuery.Where(x => x.CreationTime >= input.FromDate.Value);
        
        if (input.ToDate.HasValue)
            logsQuery = logsQuery.Where(x => x.CreationTime <= input.ToDate.Value);

        if (input.MessageType == MessageTypeFilter.SMS)
            logsQuery = logsQuery.Where(x => x.Channel == "SMS");
        else if (input.MessageType == MessageTypeFilter.Email)
            logsQuery = logsQuery.Where(x => x.Channel == "Email");

        logsQuery = logsQuery.Where(x => x.RecipientId.HasValue);

        // 3. Join & Filter Alumni
        var query = from log in logsQuery
                    join profile in profilesQuery on log.RecipientId equals profile.Id
                    select new { Log = log, Profile = profile };

        // Mandatory Branch Filter
        query = query.Where(x => x.Profile.BranchId == input.BranchId);

        // Education Filters
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

        // Execute
        var data = await AsyncExecuter.ToListAsync(query);
        var result = new SmsReportResultDto();

        if (input.ReportType == ReportType.Detailed)
        {
            result.Details = data
                .Select(x => new SmsReportDetailDto
                {
                    MessageTitle = x.Log.Subject,
                    MessageDate = x.Log.CreationTime,
                    MessageType = x.Log.Channel,
                    AlumniId = x.Profile.Id,
                    AlumniName = $"{x.Profile.JobTitle}", // Fallback Name
                    BranchId = x.Profile.BranchId,
                    BranchName = "Lookup via Branch ID"
                })
                .OrderBy(x => x.AlumniName)
                .ToList();
        }
        else
        {
            // Statistical Pivot
            var stats = data
                .Select(x => new 
                {
                    Year = x.Profile.Educations.Any() ? x.Profile.Educations.Max(e => e.GraduationYear) : 0,
                    Channel = x.Log.Channel
                })
                .GroupBy(x => x.Year)
                .Select(g => new SmsReportStatsRowDto
                {
                    GraduationYear = g.Key,
                    MobileCount = g.Count(x => x.Channel == "SMS"),
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
