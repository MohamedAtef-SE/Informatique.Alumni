
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using System.Linq;

namespace Informatique.Alumni.Guidance;

public class GuidanceSessionRuleManager : DomainService
{
    private readonly IRepository<GuidanceSessionRule, Guid> _ruleRepository;

    public GuidanceSessionRuleManager(IRepository<GuidanceSessionRule, Guid> ruleRepository)
    {
        _ruleRepository = ruleRepository;
    }

    public async Task UpdateRuleAsync(Guid branchId, TimeSpan startTime, TimeSpan endTime, int sessionDurationMinutes, List<DayOfWeek> days)
    {
        // 1. Validation Logic
        if (endTime <= startTime)
        {
            throw new UserFriendlyException("EndTime must be strictly greater than StartTime.");
        }

        var totalWindowWith = endTime - startTime;
        if (sessionDurationMinutes > totalWindowWith.TotalMinutes)
        {
             throw new UserFriendlyException($"Session Duration ({sessionDurationMinutes} mins) cannot exceed the operating window ({totalWindowWith.TotalMinutes} mins).");
        }

        // 2. Fetch or Create
        var rule = await _ruleRepository.FirstOrDefaultAsync(x => x.BranchId == branchId);
        
        if (rule == null)
        {
            rule = new GuidanceSessionRule(
                GuidGenerator.Create(),
                branchId,
                startTime,
                endTime,
                sessionDurationMinutes
            );
            
            foreach(var day in days)
            {
                rule.AddWeekDay(GuidGenerator.Create(), day);
            }
            
            await _ruleRepository.InsertAsync(rule);
        }
        else
        {
            rule.UpdateTimeWindow(startTime, endTime, sessionDurationMinutes);
            
            rule.ClearWeekDays();
            foreach(var day in days)
            {
                rule.AddWeekDay(GuidGenerator.Create(), day);
            }
            
            await _ruleRepository.UpdateAsync(rule);
        }
    }
}
