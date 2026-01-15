
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Informatique.Alumni.Profiles;
using Informatique.Alumni.Services;

namespace Informatique.Alumni.Guidance;

public class AdvisingManager : DomainService
{
    private readonly IRepository<AdvisingRequest, Guid> _requestRepository;
    private readonly IRepository<AlumniProfile, Guid> _alumniRepository;
    private readonly IServiceAccessManager _serviceAccessManager;

    // Hardcoded for now as per instructions (or assumed default)
    private const int DefaultSessionDurationMinutes = 30;

    public AdvisingManager(
        IRepository<AdvisingRequest, Guid> requestRepository,
        IRepository<AlumniProfile, Guid> alumniRepository,
        IServiceAccessManager serviceAccessManager)
    {
        _requestRepository = requestRepository;
        _alumniRepository = alumniRepository;
        _serviceAccessManager = serviceAccessManager;
    }

    public async Task<AdvisingRequest> CreateRequestAsync(
        Guid userId,
        Guid? branchId,
        Guid advisorId,
        DateTime date, 
        DateTime startTime,
        string subject,
        string? description)
    {
        // 1. Get Alumni Profile
        var alumni = await _alumniRepository.GetAsync(x => x.UserId == userId);
        if (alumni == null)
        {
            throw new UserFriendlyException("Alumni profile not found.");
        }

        // 2. Eligibility Check (ServiceAccessManager)
        if (!await _serviceAccessManager.CanCreateAdvisingRequestAsync(alumni.Id))
        {
            throw new UserFriendlyException("You are not eligible to book an advising session. Please check your card status.");
        }

        // 3. Branch Logic
        // If branchId is null, use Graduate's Branch. If provided, use that.
        var targetBranchId = branchId ?? alumni.BranchId;

        // 4. Validate Schedule & Availability
        
        // 4.1 Daily Limit: Constraint 3 logic from rules
        // "A Graduate cannot book more than ONE session on the same day."
        // We use the 'date' parameter for checking the day.
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1).AddTicks(-1);

        var hasExistingRequestToday = await _requestRepository.AnyAsync(x => 
            x.AlumniId == alumni.Id && 
            x.StartTime >= dayStart && x.StartTime <= dayEnd &&
            x.Status != AdvisingRequestStatus.Cancelled &&
            x.Status != AdvisingRequestStatus.Rejected); // Assuming Rejected doesn't count against limit? Rule says "Status != Cancelled" in example logic.

        if (hasExistingRequestToday)
        {
            throw new UserFriendlyException("You cannot book more than one session per day.");
        }

        // 4.2 Slot Availability: Constraint 2 logic
        // "The User cannot pick a time slot that is already booked by another graduate."
        // Logic: Check Repository.Any(x => x.BranchId == input.BranchId && x.Date == input.Date && x.StartTime == input.StartTime && x.Status != Cancelled).
        
        // Ensure startTime matches the date passed
        if (startTime.Date != date.Date)
        {
             // Force startTime to match the date if they differ (or throw). I'll respect startTime's date part if present, but the rule implies selection of Date + Time.
             // I will adhere to StartTime for exact collision check.
        }

        var isSlotTaken = await _requestRepository.AnyAsync(x => 
            x.BranchId == targetBranchId &&
            x.StartTime == startTime && 
            x.Status != AdvisingRequestStatus.Cancelled &&
            x.Status != AdvisingRequestStatus.Rejected);

        if (isSlotTaken)
        {
             throw new UserFriendlyException("This time slot is already booked. Please choose another one.");
        }

        // 5. Auto-Calculate EndTime
        var endTime = startTime.AddMinutes(DefaultSessionDurationMinutes);

        // 6. Create & Save
        var request = new AdvisingRequest(
            GuidGenerator.Create(),
            alumni.Id,
            targetBranchId,
            advisorId,
            startTime,
            endTime,
            subject
        );
        
        request.Description = description;
        // Status is Pending by default in constructor

        await _requestRepository.InsertAsync(request);

        return request;
    }
}
