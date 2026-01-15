using System;
using Volo.Abp.Domain.Entities.Auditing;
using System.Collections.Generic;

namespace Informatique.Alumni.Guidance;

public class GuidanceSessionRule : FullAuditedEntity<Guid>
{
    public Guid BranchId { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public int SessionDurationMinutes { get; private set; }
    
    public virtual ICollection<GuidanceSessionRuleWeekDay> WeekDays { get; private set; }

    private GuidanceSessionRule() 
    {
        WeekDays = new System.Collections.Generic.List<GuidanceSessionRuleWeekDay>();
    }

    public GuidanceSessionRule(
        Guid id, 
        Guid branchId, 
        TimeSpan startTime, 
        TimeSpan endTime, 
        int sessionDurationMinutes)
        : base(id)
    {
        BranchId = branchId;
        StartTime = startTime;
        EndTime = endTime;
        SessionDurationMinutes = sessionDurationMinutes;
        WeekDays = new System.Collections.Generic.List<GuidanceSessionRuleWeekDay>();
    }

    public void AddWeekDay(Guid dayId, DayOfWeek day)
    {
        WeekDays.Add(new GuidanceSessionRuleWeekDay(dayId, Id, day));
    }

    public void ClearWeekDays()
    {
        WeekDays.Clear();
    }
    
    public void UpdateTimeWindow(TimeSpan start, TimeSpan end, int duration)
    {
        StartTime = start;
        EndTime = end;
        SessionDurationMinutes = duration;
    }
}
