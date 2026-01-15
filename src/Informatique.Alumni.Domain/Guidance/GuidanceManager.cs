using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Guidance;

public class GuidanceManager : DomainService
{
    private readonly IRepository<GuidanceSessionRule, Guid> _ruleRepository;
    private readonly IRepository<AdvisingRequest, Guid> _requestRepository;

    public GuidanceManager(
        IRepository<GuidanceSessionRule, Guid> ruleRepository,
        IRepository<AdvisingRequest, Guid> requestRepository)
    {
        _ruleRepository = ruleRepository;
        _requestRepository = requestRepository;
    }

    public async Task ValidateSlotAsync(Guid advisorId, DateTime startTime, DateTime endTime)
    {
        if (startTime >= endTime)
        {
            throw new UserFriendlyException("Start time must be before end time.");
        }

        /* OBSOLETE: GuidanceSessionRule is now per-Branch.
        // Rule 1: Slot must be within Advisor's availability
        var dayOfWeek = startTime.DayOfWeek;
        var startDayTime = startTime.TimeOfDay;
        var endDayTime = endTime.TimeOfDay;

        var rules = await _ruleRepository.GetListAsync(x => 
            x.AdvisorId == advisorId && 
            x.DayOfWeek == dayOfWeek && 
            x.IsActive);

        var isWithinAvailability = rules.Any(r => 
            startDayTime >= r.StartTime && 
            endDayTime <= r.EndTime);

        if (!isWithinAvailability)
        {
            throw new UserFriendlyException("Selected slot is outside the advisor's defined availability.");
        }
        */

        // Rule 2: Advisor must not have another accepted or requested request at that time
        var hasConflict = await _requestRepository.AnyAsync(x => 
            x.AdvisorId == advisorId &&
            x.Status != AdvisingRequestStatus.Rejected &&
            x.Status != AdvisingRequestStatus.Cancelled &&
            ((startTime >= x.StartTime && startTime < x.EndTime) || 
             (endTime > x.StartTime && endTime <= x.EndTime) ||
             (startTime <= x.StartTime && endTime >= x.EndTime)));

        if (hasConflict)
        {
            throw new UserFriendlyException("Advisor has a conflicting session at this time.");
        }
    }
}
