using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.Domain.Repositories;
using Informatique.Alumni.Payment;
using Informatique.Alumni.Profiles;

namespace Informatique.Alumni.Syndicates;

public class SyndicateManager : DomainService
{
    private readonly IRepository<SyndicateSubscription, Guid> _subscriptionRepository;
    private readonly IRepository<AlumniProfile, Guid> _alumniProfileRepository;

    public SyndicateManager(
        IRepository<SyndicateSubscription, Guid> subscriptionRepository,
        IRepository<AlumniProfile, Guid> alumniProfileRepository) 
    {
        _subscriptionRepository = subscriptionRepository;
        _alumniProfileRepository = alumniProfileRepository;
    }

    public async Task<SyndicateSubscription> CreateRequestAsync(Guid alumniId, Guid syndicateId, decimal feeAmount)
    {
        // Singleton Constraint: Check for any active request
        var activeRequest = await _subscriptionRepository.AnyAsync(x => 
            x.AlumniId == alumniId && 
            x.Status != SyndicateStatus.Received && 
            x.Status != SyndicateStatus.Rejected);

        if (activeRequest)
        {
            throw new UserFriendlyException("You already have an active syndicate request. Please complete or cancel it first.");
        }

        return new SyndicateSubscription(GuidGenerator.Create(), alumniId, syndicateId)
        {
            FeeAmount = feeAmount,
            PaymentStatus = PaymentStatus.NotPaid
        };
    }

    public async Task<decimal> PayRequestAsync(SyndicateSubscription subscription, PaymentGatewayType gatewayType)
    {
        if (subscription.PaymentStatus == PaymentStatus.Paid)
        {
            throw new UserFriendlyException("Request is already paid.");
        }

        var profile = await _alumniProfileRepository.GetAsync(subscription.AlumniId);
        
        // Split Payment Logic
        decimal walletDeduction = 0;
        decimal gatewayAmount = subscription.FeeAmount;

        if (profile.WalletBalance > 0)
        {
            if (profile.WalletBalance >= subscription.FeeAmount)
            {
                walletDeduction = subscription.FeeAmount;
                gatewayAmount = 0;
            }
            else
            {
                walletDeduction = profile.WalletBalance;
                gatewayAmount = subscription.FeeAmount - walletDeduction;
            }
        }

        // Processing
        if (walletDeduction > 0)
        {
            profile.DeductWallet(walletDeduction);
            subscription.PaidByWallet = walletDeduction;
        }

        subscription.PaidByGateway = gatewayAmount;
        subscription.PaymentMethod = gatewayType;

        // If fully paid by wallet, mark as Paid immediately
        if (gatewayAmount == 0)
        {
            subscription.PaymentStatus = PaymentStatus.Paid;
        }
        else
        {
            // Pending Gateway Payment
            // In a real scenario, we return this amount to the AppService to initiate Gateway Transaction
             subscription.PaymentStatus = PaymentStatus.Pending; // Waiting for WebHook/Callback
        }
        
        await _alumniProfileRepository.UpdateAsync(profile);
        
        return gatewayAmount;
    }

    public async Task CancelRequestAsync(SyndicateSubscription subscription)
    {
        if (subscription.Status == SyndicateStatus.Received)
        {
             throw new UserFriendlyException("Cannot cancel a received request.");
        }

        // Refund Logic
        if (subscription.PaidByWallet > 0)
        {
            var profile = await _alumniProfileRepository.GetAsync(subscription.AlumniId);
            profile.AddCredit(subscription.PaidByWallet);
            await _alumniProfileRepository.UpdateAsync(profile);
        }

        if (subscription.PaidByGateway > 0)
        {
            // Log for Manual Refund
            // In a real system: _auditLog.Log("Manual Refund Required", subscription.PaidByGateway);
            // Here we just set a note or assume external process
            subscription.AdminNotes += $" [System]: Auto-Refunded {subscription.PaidByWallet} to Wallet. Manual Refund of {subscription.PaidByGateway} required.";
        }

        subscription.Status = SyndicateStatus.Rejected; // or Cancelled if enum had it, using Rejected as terminal
    }

    public void VerifyRequirementCompletion(SyndicateSubscription subscription, Syndicate syndicate)
    {
        if (string.IsNullOrWhiteSpace(syndicate.Requirements)) return;

        var requiredSpecs = syndicate.Requirements.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
        var uploadedSpecs = subscription.Documents.Select(x => x.RequirementName).ToList();

        foreach (var spec in requiredSpecs)
        {
            if (!uploadedSpecs.Contains(spec, StringComparer.OrdinalIgnoreCase))
            {
                throw new UserFriendlyException($"Missing mandatory document: {spec}");
            }
        }
    }

    public void ValidateStatusTransition(SyndicateStatus currentStatus, SyndicateStatus newStatus, bool isPaid)
    {
        // 1. Payment Guard: "The Request cannot proceed to processing until fees are paid."
        // Transition: New (Pending) -> In Progress (Reviewing)
        if (currentStatus == SyndicateStatus.Pending && newStatus == SyndicateStatus.Reviewing)
        {
            if (!isPaid)
            {
                throw new UserFriendlyException("Cannot start processing until fees are paid.");
            }
        }

        // 2. Logical Flow Guards
        // Ensure status changes follow a logical flow (e.g., cannot jump from New to Received).
        
        // Rule: Received status can only be reached from CardReady.
        if (newStatus == SyndicateStatus.Received && currentStatus != SyndicateStatus.CardReady)
        {
            throw new UserFriendlyException($"Invalid transition. Cannot move to 'Received' from '{currentStatus}'. Request must be 'Ready for Pickup' first.");
        }

        // Rule: Cannot jump directly from New -> CardReady (skipping InProgress/Reviewing)
        if (currentStatus == SyndicateStatus.Pending && newStatus == SyndicateStatus.CardReady)
        {
            throw new UserFriendlyException("Invalid transition. Request must be processed (In Progress) before being Ready for Pickup.");
        }
    }
}
