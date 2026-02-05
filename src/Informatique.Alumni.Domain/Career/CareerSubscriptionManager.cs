using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Profiles;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Career;

public class CareerSubscriptionManager : DomainService
{
    private readonly IRepository<CareerService, Guid> _serviceRepository;
    private readonly IRepository<AlumniCareerSubscription, Guid> _subscriptionRepository;
    private readonly IRepository<CareerServiceTimeslot, Guid> _timeslotRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly MembershipManager _membershipManager;
    private readonly Volo.Abp.Guids.IGuidGenerator _guidGenerator;

    public CareerSubscriptionManager(
        IRepository<CareerService, Guid> serviceRepository,
        IRepository<AlumniCareerSubscription, Guid> subscriptionRepository,
        IRepository<CareerServiceTimeslot, Guid> timeslotRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        MembershipManager membershipManager,
        Volo.Abp.Guids.IGuidGenerator guidGenerator)
    {
        _serviceRepository = serviceRepository;
        _subscriptionRepository = subscriptionRepository;
        _timeslotRepository = timeslotRepository;
        _profileRepository = profileRepository;
        _membershipManager = membershipManager;
        _guidGenerator = guidGenerator;
    }

    public async Task<AlumniCareerSubscription> SubscribeAsync(Guid serviceId, Guid userId, Guid timeslotId, CareerPaymentMethod paymentMethod)
    {
        // 1. Get profile by UserId first
        var profile = await _profileRepository.GetAsync(p => p.UserId == userId);
        var profileId = profile.Id;

        // 2. Membership Gating (uses ProfileId, not UserId)
        if (!await _membershipManager.IsActiveAsync(profileId))
        {
            throw new BusinessException("Career:MembershipNotActive");
        }

        // 3. Load Data
        var service = await _serviceRepository.GetAsync(serviceId);
        var timeslot = await _timeslotRepository.GetAsync(timeslotId);
        
        // 3. Deadline Check
        if (DateTime.Now > service.LastSubscriptionDate)
        {
            throw new BusinessException("Career:SubscriptionDeadlinePassed");
        }

        // 4. Capacity & Integrity Check
        if (timeslot.CareerServiceId != serviceId)
        {
            throw new BusinessException("Career:InvalidTimeslotForActivity");
        }
        
        if (timeslot.CurrentCount >= timeslot.Capacity)
        {
            throw new BusinessException("Career:TimeslotFull");
        }

        // 5. Time Overlap & Single Subscription Check
        var existingSubs = await _subscriptionRepository.GetListAsync(s => s.AlumniId == profileId && s.PaymentStatus != CareerPaymentStatus.Cancelled);
        
        // Check if already subscribed to THIS service (Rule: One active session per service)
        if (existingSubs.Any(s => s.CareerServiceId == serviceId))
        {
             throw new BusinessException("Career:AlreadySubscribedToService");
        }

        foreach (var sub in existingSubs)
        {
            var subTimeslot = await _timeslotRepository.GetAsync(sub.TimeslotId);
            
            // Check Date & Time overlap
            if (subTimeslot.Date.Date == timeslot.Date.Date)
            {
                bool overlap = (timeslot.StartTime < subTimeslot.EndTime) && (timeslot.EndTime > subTimeslot.StartTime);
                if (overlap)
                {
                     throw new BusinessException("Career:TimeOverlapDetected");
                }
            }
        }

        // 6. Create Subscription
        var subscription = new AlumniCareerSubscription(_guidGenerator.Create(), serviceId, profileId);

        // 7. Payment Logic
        decimal amountToPay = service.HasFees ? service.FeeAmount : 0;
        
        if (service.HasFees) 
        {
            if (paymentMethod == CareerPaymentMethod.Wallet)
            {
                if (profile.WalletBalance < amountToPay)
                {
                     throw new BusinessException("Career:InsufficientWalletBalance");
                }
                
                profile.DeductWallet(amountToPay);
                subscription.SetFinancialDetails(timeslotId, CareerPaymentMethod.Wallet, amountToPay);
                subscription.MarkAsPaid();
            }
            else
            {
                // Cash or Gateway - requires separate payment processing
                subscription.SetFinancialDetails(timeslotId, paymentMethod, amountToPay);
            }
        }
        else
        {
             // Free Service
             subscription.SetFinancialDetails(timeslotId, CareerPaymentMethod.Wallet, 0); // Method irrelevant for free
             subscription.MarkAsPaid();
        }

        // 8. Finalize
        timeslot.IncrementCount();
        await _profileRepository.UpdateAsync(profile);
        await _timeslotRepository.UpdateAsync(timeslot);
        await _subscriptionRepository.InsertAsync(subscription);
        
        return subscription;
    }

    public async Task CancelSubscriptionAsync(Guid serviceId, Guid alumniId)
    {
        // Logic change: Find subscription by Service + Alumni (assuming 1 per service? Or lookup needed?)
        // Assuming user passes ServiceId, we find the active subscription.
        
        var sub = await _subscriptionRepository.GetAsync(s => s.CareerServiceId == serviceId && s.AlumniId == alumniId && s.PaymentStatus != CareerPaymentStatus.Cancelled);
        
        var timeslot = await _timeslotRepository.GetAsync(sub.TimeslotId);
        
        // Deadline Check
        var startDateTime = timeslot.Date.Date.Add(timeslot.StartTime);
        if (DateTime.Now > startDateTime)
        {
            throw new BusinessException("Career:CannotCancelPastService");
        }

        // Refund Logic
        if (sub.PaymentMethod == CareerPaymentMethod.Wallet && sub.PaymentStatus == CareerPaymentStatus.Paid && sub.AmountPaid > 0)
        {
            var profile = await _profileRepository.GetAsync(sub.AlumniId);
            profile.AddCredit(sub.AmountPaid);
            await _profileRepository.UpdateAsync(profile);
            sub.MarkAsCancelled(isRefunded: true);
        }
        else
        {
            sub.MarkAsCancelled(isRefunded: false);
        }

        timeslot.DecrementCount();
        await _timeslotRepository.UpdateAsync(timeslot);
        await _subscriptionRepository.UpdateAsync(sub);
    }

    public async Task<CareerSubscriptionCancelledEto> ForceCancelAsync(Guid subscriptionId, string cancellationReason)
    {
        // 1. Load Subscription and Dependencies
        var subscription = await _subscriptionRepository.GetAsync(subscriptionId);
        var timeslot = await _timeslotRepository.GetAsync(subscription.TimeslotId);
        var service = await _serviceRepository.GetAsync(subscription.CareerServiceId);
        
        // 2. Validate: Cannot cancel historical subscriptions
        var timeslotEndDateTime = timeslot.Date.Date.Add(timeslot.EndTime);
        if (DateTime.Now > timeslotEndDateTime)
        {
            throw new BusinessException("Career:CannotCancelHistoricalSubscription")
                .WithData("ServiceName", service.NameEn)
                .WithData("TimeslotDate", timeslot.Date.ToString("yyyy-MM-dd"));
        }

        // 3. Refund Logic
        bool wasRefunded = false;
        decimal refundAmount = 0;

        if (subscription.PaymentStatus == CareerPaymentStatus.Paid && subscription.AmountPaid > 0)
        {
            if (subscription.PaymentMethod == CareerPaymentMethod.Wallet)
            {
                // Auto-refund to wallet
                var profile = await _profileRepository.GetAsync(subscription.AlumniId);
                profile.AddCredit(subscription.AmountPaid);
                await _profileRepository.UpdateAsync(profile);
                
                wasRefunded = true;
                refundAmount = subscription.AmountPaid;
            }
            else
            {
                // Gateway/Cash: Log but don't auto-refund
                wasRefunded = false;
                refundAmount = subscription.AmountPaid;
                // Note: Actual refund must be processed manually outside the system
            }
        }

        // 4. Update Subscription Status
        subscription.MarkAsCancelled(wasRefunded);

        // 5. Decrement Timeslot Count
        timeslot.DecrementCount();

        // 6. Save Changes
        await _timeslotRepository.UpdateAsync(timeslot);
        await _subscriptionRepository.UpdateAsync(subscription);

        // 7. Create Event for Notification
        var eventData = new CareerSubscriptionCancelledEto
        {
            SubscriptionId = subscriptionId,
            AlumniId = subscription.AlumniId,
            CareerServiceId = service.Id,
            ServiceName = service.NameEn,
            CancellationReason = cancellationReason,
            WasRefunded = wasRefunded,
            RefundAmount = refundAmount
        };

        return eventData;
    }
}
