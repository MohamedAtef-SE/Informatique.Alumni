
using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Guidance;

public class GuidanceSessionRuleWeekDay : Entity<Guid>
{
    public Guid RuleId { get; set; }
    public DayOfWeek Day { get; set; }

    private GuidanceSessionRuleWeekDay() { }

    public GuidanceSessionRuleWeekDay(Guid id, Guid ruleId, DayOfWeek day)
        : base(id)
    {
        RuleId = ruleId;
        Day = day;
    }
}
