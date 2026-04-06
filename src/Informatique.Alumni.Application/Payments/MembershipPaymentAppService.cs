using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Informatique.Alumni.Payments;

[Authorize]
public class MembershipPaymentAppService : ApplicationService, IMembershipPaymentAppService
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<SubscriptionFee, Guid> _subscriptionFeeRepository;
    private readonly IRepository<AssociationRequest, Guid> _requestRepository;
    private readonly IRepository<Informatique.Alumni.Branches.Branch, Guid> _branchRepository;

    public MembershipPaymentAppService(
        IPaymentGateway paymentGateway,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<SubscriptionFee, Guid> subscriptionFeeRepository,
        IRepository<AssociationRequest, Guid> requestRepository,
        IRepository<Informatique.Alumni.Branches.Branch, Guid> branchRepository)
    {
        _paymentGateway = paymentGateway;
        _profileRepository = profileRepository;
        _subscriptionFeeRepository = subscriptionFeeRepository;
        _requestRepository = requestRepository;
        _branchRepository = branchRepository;
    }

    public async Task<MembershipCheckoutDto> ProcessDigitalCheckoutAsync()
    {
        if (CurrentUser.Id == null)
        {
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        // 1. Identify the authenticated user's Alumni Profile
        var profile = await _profileRepository.FirstOrDefaultAsync(p => p.UserId == CurrentUser.Id);
        if (profile == null)
        {
            throw new UserFriendlyException("Alumni profile not found for the current user. Please complete registration.");
        }

        var hasActiveSubscription = await _requestRepository.AnyAsync(x => 
            x.AlumniId == profile.Id && 
            x.Status == MembershipRequestStatus.Approved && 
            x.ValidityEndDate >= Clock.Now);

        if (hasActiveSubscription)
        {
            throw new UserFriendlyException("Your membership is already completely Active! No payment is required.");
        }

        // 2. Resolve the Subscription Fee
        var fees = await _subscriptionFeeRepository.GetListAsync();
        var currentFee = fees.FirstOrDefault(); // Standard fallback
        if (currentFee == null)
        {
            throw new UserFriendlyException("Configuration Error: No active subscription fees are defined in the system.");
        }

        // 3. Initiate the abstract Payment Gateway (Stripe/Paymob simulation)
        var paymentResult = await _paymentGateway.ProcessPaymentAsync(currentFee.Amount, "EGP", profile.Id.ToString());

        if (paymentResult.Success)
        {
            if (profile.Status != AlumniStatus.Active)
            {
                // 4. Securely transition the profile from Pending -> Active automatically
                profile.Approve();
                await _profileRepository.UpdateAsync(profile);
            }

            // Ensure we use a valid branch to avoid Foreign Key crashes
            var defaultBranch = await _branchRepository.FirstOrDefaultAsync();
            var branchId = defaultBranch?.Id ?? Guid.Empty;

            // 5. Generate the Subscription Record (AssociationRequest) to physically activate membership privileges
            var request = new AssociationRequest(
                GuidGenerator.Create(),
                profile.Id,
                currentFee.Id,
                paymentResult.TransactionId, // IdempotencyKey bound to bank txn
                branchId, // Valid dynamically resolved Branch
                Clock.Now,
                new DateTime(Clock.Now.Year, 12, 31), // End of current year
                DeliveryMethod.OfficePickup,
                0,
                0,
                currentFee.Amount,
                null
            );

            request.MarkAsPaid();
            request.Approve();

            await _requestRepository.InsertAsync(request);
        }

        return new MembershipCheckoutDto
        {
            Success = paymentResult.Success,
            TransactionId = paymentResult.TransactionId,
            ErrorMessage = paymentResult.ErrorMessage
        };
    }
}
