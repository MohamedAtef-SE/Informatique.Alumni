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
        // Singleton Rule: Check for any active request
        // Active = Anything NOT Received AND NOT Rejected
        var activeRequest = await _subscriptionRepository.AnyAsync(x => 
            x.AlumniId == alumniId && 
            x.Status != SyndicateStatus.Received && 
            x.Status != SyndicateStatus.Rejected);

        if (activeRequest)
        {
            throw new UserFriendlyException("You already have an active syndicate request. Please complete or cancel it first.");
        }

        return new SyndicateSubscription(GuidGenerator.Create(), alumniId, syndicateId, feeAmount);
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
