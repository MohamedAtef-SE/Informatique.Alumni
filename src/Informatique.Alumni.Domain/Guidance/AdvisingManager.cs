using System;
using System.Linq;
using System.Collections.Generic;
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
        Guid alumniId,
        Guid? branchId,
        Guid advisorId,
        DateTime selectedDate, 
        TimeSpan requestedStartTime,
        string subject,
        string? description)
    {
        // 1. Get Alumni Profile
        var alumni = await _alumniRepository.GetAsync(alumniId);
        if (alumni == null)
        {
             throw new UserFriendlyException("Alumni profile not found.");
        }

        var targetBranchId = branchId ?? alumni.BranchId;

        // 2. Load Branch Rules with Fallback
        var rules = await _ruleRepository.WithDetailsAsync(x => x.WeekDays);
        var rule = rules.FirstOrDefault(x => x.BranchId == targetBranchId);

        var startTime = rule?.StartTime ?? new TimeSpan(9, 0, 0);
        var endTime = rule?.EndTime ?? new TimeSpan(17, 0, 0);
        var duration = rule?.SessionDurationMinutes ?? 30;
        var validWorkDays = rule?.WeekDays?.Select(x => x.Day).ToList() ?? new List<DayOfWeek> { 
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, 
            DayOfWeek.Thursday, DayOfWeek.Friday 
        };

        // 3. Calculate Timings
        var startDateTime = selectedDate.Date.Add(requestedStartTime);
        var endDateTime = startDateTime.AddMinutes(duration);

        // 4. Validate Rules
        // 4.1 Time Window Check
        if (requestedStartTime < startTime || endDateTime.TimeOfDay > endTime)
        {
             throw new UserFriendlyException($@"Selected time is outside the allowed advising hours ({startTime:hh\:mm} - {endTime:hh\:mm}).");
        }

        // 4.2 Valid Day Check
        if (!validWorkDays.Contains(selectedDate.DayOfWeek))
        {
             throw new UserFriendlyException($"Advising sessions are not available on {selectedDate.DayOfWeek}.");
        }
        
        // 5. Daily Limit Logic (Max 1 per day)
        var dayStart = selectedDate.Date;
        var dayEnd = dayStart.AddDays(1).AddTicks(-1);

        var hasExistingRequest = await _requestRepository.AnyAsync(x => 
            x.AlumniId == alumni.Id && 
            x.StartTime >= dayStart && x.StartTime <= dayEnd &&
            x.Status != AdvisingRequestStatus.Rejected &&
            x.Status != AdvisingRequestStatus.Canceled);

        if (hasExistingRequest)
        {
            throw new UserFriendlyException("Limit reached: You can only book one advising session per day.");
        }

        // 6. Slot Availability (Conflict Check)
        var isSlotTaken = await _requestRepository.AnyAsync(x => 
            x.BranchId == targetBranchId &&
            x.AdvisorId == advisorId && 
            x.Status != AdvisingRequestStatus.Rejected &&
            x.Status != AdvisingRequestStatus.Canceled &&
            ((x.StartTime <= startDateTime && x.EndTime > startDateTime) || 
             (x.StartTime < endDateTime && x.EndTime >= endDateTime) || 
             (x.StartTime >= startDateTime && x.EndTime <= endDateTime))
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
