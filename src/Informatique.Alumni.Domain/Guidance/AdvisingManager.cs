using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Informatique.Alumni.Profiles;

namespace Informatique.Alumni.Guidance;

public class AdvisingManager : DomainService
{
    private readonly IRepository<AdvisingRequest, Guid> _requestRepository;
    private readonly IRepository<AlumniProfile, Guid> _alumniRepository;
    private readonly IRepository<GuidanceSessionRule, Guid> _ruleRepository;

    public AdvisingManager(
        IRepository<AdvisingRequest, Guid> requestRepository,
        IRepository<AlumniProfile, Guid> alumniRepository,
        IRepository<GuidanceSessionRule, Guid> ruleRepository)
    {
        _requestRepository = requestRepository;
        _alumniRepository = alumniRepository;
        _ruleRepository = ruleRepository;
    }

    public async Task<AdvisingRequest> CreateRequestAsync(
        Guid userId,
        Guid? branchId,
        Guid advisorId,
        DateTime selectedDate, 
        TimeSpan requestedStartTime,
        string subject,
        string? description)
    {
        // 1. Get Alumni Profile
        var alumni = await _alumniRepository.GetAsync(x => x.UserId == userId);
        if (alumni == null)
        {
             // Fallback for demo or strict check
             throw new UserFriendlyException("Alumni profile not found.");
        }

        var targetBranchId = branchId ?? alumni.BranchId;

        // 2. Load Branch Rules
        var rules = await _ruleRepository.GetListAsync(x => x.BranchId == targetBranchId);
        var rule = rules.FirstOrDefault(); // Assuming one rule per branch for simplicity, or select best match

        if (rule == null)
        {
            throw new UserFriendlyException("No advising rules defined for this branch. Cannot book session.");
        }

        // 3. Calculate Timings
        // Use the Rule's duration
        var sessionDuration = rule.SessionDurationMinutes;
        var startDateTime = selectedDate.Date.Add(requestedStartTime);
        var endDateTime = startDateTime.AddMinutes(sessionDuration);

        // 4. Validate Rules
        // 4.1 Time Window Check
        if (requestedStartTime < rule.StartTime || endDateTime.TimeOfDay > rule.EndTime)
        {
             throw new UserFriendlyException($"Selected time is outside the allowed advising hours ({rule.StartTime} - {rule.EndTime}).");
        }

        // 4.2 Valid Day Check
        // Need to check if rule.WeekDays contains the selected day
        if (rule.WeekDays != null && rule.WeekDays.Any())
        {
            if (!rule.WeekDays.Any(x => x.Day == selectedDate.DayOfWeek))
            {
                 throw new UserFriendlyException($"Advising sessions are not available on {selectedDate.DayOfWeek}.");
            }
        }
        
        // 5. Daily Limit Logic (Max 1 per day)
        var dayStart = selectedDate.Date;
        var dayEnd = dayStart.AddDays(1).AddTicks(-1);

        var hasExistingRequest = await _requestRepository.AnyAsync(x => 
            x.AlumniId == alumni.Id && 
            x.StartTime >= dayStart && x.StartTime <= dayEnd &&
            x.Status != AdvisingRequestStatus.Rejected &&
            x.Status != AdvisingRequestStatus.Cancelled);

        if (hasExistingRequest)
        {
            throw new UserFriendlyException("Limit reached: You can only book one advising session per day.");
        }

        // 6. Slot Availability (Conflict Check)
        // Check for overlap [Start, End]
        var isSlotTaken = await _requestRepository.AnyAsync(x => 
            x.BranchId == targetBranchId &&
            x.AdvisorId == advisorId && // Assuming conflict is per Advisor
            x.Status != AdvisingRequestStatus.Rejected &&
            x.Status != AdvisingRequestStatus.Cancelled &&
            ((x.StartTime <= startDateTime && x.EndTime > startDateTime) || // Starts during existing
             (x.StartTime < endDateTime && x.EndTime >= endDateTime) || // Ends during existing
             (x.StartTime >= startDateTime && x.EndTime <= endDateTime)) // Enclosed
        );

        if (isSlotTaken)
        {
             throw new UserFriendlyException("The selected time slot is already booked. Please choose another time.");
        }

        // 7. Create Request
        var request = new AdvisingRequest(
            GuidGenerator.Create(),
            alumni.Id,
            targetBranchId,
            advisorId,
            startDateTime,
            endDateTime,
            subject
        );
        request.Description = description;

        return await _requestRepository.InsertAsync(request);
    }
}
