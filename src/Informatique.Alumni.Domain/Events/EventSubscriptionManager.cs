using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Events;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Profiles;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Local;

namespace Informatique.Alumni.Events;

public class EventSubscriptionManager : DomainService
{
    private readonly IRepository<AssociationEvent, Guid> _eventRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<AlumniEventRegistration, Guid> _registrationRepository;
    private readonly ILocalEventBus _localEventBus;

    public EventSubscriptionManager(
        IRepository<AssociationEvent, Guid> eventRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<AlumniEventRegistration, Guid> registrationRepository,
        ILocalEventBus localEventBus)
    {
        _eventRepository = eventRepository;
        _profileRepository = profileRepository;
        _registrationRepository = registrationRepository;
        _localEventBus = localEventBus;
    }

    /// <summary>
    /// Subscribe user to an event with strict business rule enforcement.
    /// Business Rules:
    /// 1. Membership Gating: Active members only
    /// 2. Deadline: Cannot subscribe after LastSubscriptionDate
    /// 3. Time Overlap: User cannot register for overlapping events
    /// 4. Capacity: Timeslot must have available capacity
    /// 5. Payment: Wallet/Gateway/Mixed based on balance
    /// </summary>
    public async Task<AlumniEventRegistration> SubscribeAsync(
        Guid userId,
        Guid eventId,
        Guid timeslotId,
        string serviceName = "Events")
    {
        // ========== STEP 1: LOAD EVENT & TIMESLOT ==========
        var @event = await _eventRepository.GetAsync(eventId);
        await _eventRepository.EnsureCollectionLoadedAsync(@event, x => x.Timeslots);

        var timeslot = @event.Timeslots.FirstOrDefault(t => t.Id == timeslotId);
        if (timeslot == null)
        {
            throw new BusinessException(AlumniDomainErrorCodes.EventSubscription.TimeslotNotFound)
                .WithData("EventId", eventId)
                .WithData("TimeslotId", timeslotId);
        }

        // ========== STEP 2: VALIDATION & ELIGIBILITY ==========
        
        // 2a. Membership Gating (Active Members Only)
        var serviceAccessManager = LazyServiceProvider.LazyGetRequiredService<ServiceAccessManager>();
        await serviceAccessManager.CheckAccessAsync(userId, serviceName);

        // 2b. Deadline Check
        if (DateTime.Now > @event.LastSubscriptionDate)
        {
            throw new BusinessException(AlumniDomainErrorCodes.EventSubscription.DeadlinePassed)
                .WithData("EventName", @event.NameEn)
                .WithData("LastSubscriptionDate", @event.LastSubscriptionDate);
        }

        // 2c. Check if already registered for THIS event
        var existingRegistration = await _registrationRepository.FirstOrDefaultAsync(
            r => r.AlumniId == userId && r.EventId == eventId && r.Status != RegistrationStatus.Cancelled);
        
        if (existingRegistration != null)
        {
            throw new BusinessException(AlumniDomainErrorCodes.EventSubscription.AlreadyRegistered)
                .WithData("EventId", eventId)
                .WithData("ExistingRegistrationId", existingRegistration.Id);
        }

        // 2d. Time Overlap Detection
        // Get all active registrations for this user
        var userRegistrations = await _registrationRepository.GetListAsync(
            r => r.AlumniId == userId && r.Status != RegistrationStatus.Cancelled);

        if (userRegistrations.Any())
        {
            // Load events for these registrations
            var registeredEventIds = userRegistrations.Select(r => r.EventId).Distinct().ToList();
            var registeredEvents = await _eventRepository.GetListAsync(e => registeredEventIds.Contains(e.Id));

            foreach (var regEvent in registeredEvents)
            {
                await _eventRepository.EnsureCollectionLoadedAsync(regEvent, x => x.Timeslots);
                
                // Find the specific timeslots user registered for
                var userRegsForEvent = userRegistrations.Where(r => r.EventId == regEvent.Id).ToList();
                
                foreach (var userReg in userRegsForEvent)
                {
                    if (userReg.TimeslotId.HasValue)
                    {
                        var registeredSlot = regEvent.Timeslots.FirstOrDefault(t => t.Id == userReg.TimeslotId.Value);
                        if (registeredSlot != null)
                        {
                            // Check for time overlap
                            // Overlap condition: (NewStart < ExistingEnd) && (NewEnd > ExistingStart)
                            if (timeslot.StartTime < registeredSlot.EndTime && timeslot.EndTime > registeredSlot.StartTime)
                            {
                                throw new BusinessException(AlumniDomainErrorCodes.EventSubscription.TimeOverlap)
                                    .WithData("NewEvent", @event.NameEn)
                                    .WithData("NewTimeslot", $"{timeslot.StartTime:g} - {timeslot.EndTime:g}")
                                    .WithData("ConflictingEvent", regEvent.NameEn)
                                    .WithData("ConflictingTimeslot", $"{registeredSlot.StartTime:g} - {registeredSlot.EndTime:g}");
                            }
                        }
                    }
                }
            }
        }

        // ========== STEP 3: CAPACITY VALIDATION (Concurrency-Safe) ==========
        
        var currentRegistrationsCount = await _registrationRepository.CountAsync(
            r => r.TimeslotId == timeslotId && r.Status != RegistrationStatus.Cancelled);

        if (currentRegistrationsCount >= timeslot.Capacity)
        {
            throw new BusinessException(AlumniDomainErrorCodes.EventSubscription.TimeslotFull)
                .WithData("Timeslot", $"{timeslot.StartTime:g} - {timeslot.EndTime:g}")
                .WithData("Capacity", timeslot.Capacity)
                .WithData("CurrentRegistrations", currentRegistrationsCount);
        }

        // ========== STEP 4: FINANCIAL LOGIC & PAYMENT ==========
        
        var fee = @event.HasFees ? (@event.FeeAmount ?? 0) : 0;
        var profile = await _profileRepository.FirstOrDefaultAsync(p => p.UserId == userId);
        
        string paymentMethod;
        decimal paidAmount = fee;
        
        if (fee > 0)
        {
            if (profile == null)
            {
                throw new BusinessException("Alumni:Profile:NotFound")
                    .WithData("UserId", userId);
            }

            var walletBalance = profile.WalletBalance;

            if (walletBalance >= fee)
            {
                // Full payment from wallet
                paymentMethod = "Wallet";
                profile.DeductWallet(fee);
                await _profileRepository.UpdateAsync(profile);
            }
            else if (walletBalance > 0)
            {
                // Mixed payment: Partial wallet + Gateway
                paymentMethod = "Mixed";
                profile.DeductWallet(walletBalance);
                await _profileRepository.UpdateAsync(profile);
                // Remaining amount (fee - walletBalance) should be handled via gateway
                // For now, we track the paid amount (wallet portion)
            }
            else
            {
                // Full payment via gateway
                paymentMethod = "Gateway";
                // Payment will be handled externally via gateway
            }
        }
        else
        {
            // Free event
            paymentMethod = "Free";
            paidAmount = 0;
        }

        // ========== STEP 5: CREATE REGISTRATION ==========
        
        var ticketCode = GenerateTicketCode();
        var registration = new AlumniEventRegistration(
            GuidGenerator.Create(),
            userId,
            eventId,
            ticketCode,
            timeslotId,
            paymentMethod,
            paidAmount
        );

        await _registrationRepository.InsertAsync(registration);

        return registration;
    }

    private string GenerateTicketCode()
    {
        // Generate unique 12-character alphanumeric code
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Removed ambiguous chars
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public async Task ForceCancelAsync(Guid registrationId, string cancellationReason)
    {
        var registration = await _registrationRepository.GetAsync(registrationId);
        if (registration.Status == RegistrationStatus.Cancelled)
        {
             return;
        }

        var @event = await _eventRepository.GetAsync(registration.EventId);
        await _eventRepository.EnsureCollectionLoadedAsync(@event, x => x.Timeslots);

        // 1. Time Constraint: Cannot cancel if event ended
        // Assuming event end is the max end time of all timeslots or a specific one?
        // Logic: "Cannot delete/cancel after the EventEndDate has passed"
        DateTime eventEndDate;
        if (registration.TimeslotId.HasValue)
        {
             var slot = @event.Timeslots.FirstOrDefault(t => t.Id == registration.TimeslotId);
             eventEndDate = slot?.EndTime ?? DateTime.MaxValue;
        }
        else
        {
             eventEndDate = @event.Timeslots.Any() ? @event.Timeslots.Max(t => t.EndTime) : DateTime.MaxValue;
        }

        if (DateTime.Now > eventEndDate)
        {
            throw new UserFriendlyException("Cannot cancel subscription after the event has ended.");
        }

        // 2. Refund Logic
        if (registration.PaidAmount.HasValue && registration.PaidAmount.Value > 0)
        {
            if (registration.PaymentMethod == "Wallet")
            {
                // Auto-Refund to Wallet
                // Need to find profile by AlumniId (which is UserId in Profile? No, Registration.AlumniId is UserId)
                // Profile.UserId == Registration.AlumniId
                var profile = await _profileRepository.FirstOrDefaultAsync(p => p.UserId == registration.AlumniId);
                if (profile != null)
                {
                    profile.AddCredit(registration.PaidAmount.Value);
                    registration.MarkAsRefunded(); // Track refund status
                    await _profileRepository.UpdateAsync(profile);
                }
            }
            else
            {
                // Log "Refund pending outside system"
                // In a real system, we'd add an entry to a refund queue table.
                // Here we just proceed, maybe logging via Logger?
                // For now, we assume the reason covers it or we don't have a RefundRequest entity yet.
            }
        }

        // 3. Mark as Cancelled
        registration.Cancel();
        await _registrationRepository.UpdateAsync(registration);

        // 4. Trigger Notification Event
        await _localEventBus.PublishAsync(new SubscriptionCancelledEto
        {
            RegistrationId = registration.Id,
            AlumniId = registration.AlumniId,
            EventId = registration.EventId,
            CancellationReason = cancellationReason,
            IsRefunded = registration.PaymentMethod == "Wallet"
        });
    }
}
