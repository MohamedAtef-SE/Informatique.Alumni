using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Informatique.Alumni.Membership;

public class MembershipManager : DomainService
{
    private readonly IRepository<SubscriptionFee, Guid> _feeRepository;
    private readonly IRepository<AssociationRequest, Guid> _requestRepository;
    private readonly IRepository<PaymentTransaction, Guid> _paymentRepository;
    private readonly IRepository<Informatique.Alumni.Profiles.AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<MembershipFeeConfig, Guid> _feeConfigRepository;

    public MembershipManager(
        IRepository<SubscriptionFee, Guid> feeRepository,
        IRepository<AssociationRequest, Guid> requestRepository,
        IRepository<PaymentTransaction, Guid> paymentRepository,
        IRepository<Informatique.Alumni.Profiles.AlumniProfile, Guid> profileRepository,
        IRepository<MembershipFeeConfig, Guid> feeConfigRepository)
    {
        _feeRepository = feeRepository;
        _requestRepository = requestRepository;
        _paymentRepository = paymentRepository;
        _profileRepository = profileRepository;
        _feeConfigRepository = feeConfigRepository;
    }

    /// <summary>
    /// Business Logic: Validate subscription fee is available for new memberships
    /// </summary>
    public async Task<SubscriptionFee> ValidateSubscriptionFeeAsync(Guid subscriptionFeeId)
    {
        var fee = await _feeRepository.GetAsync(subscriptionFeeId);

        if (!fee.IsCurrentlyValid())
        {
            var ex = new BusinessException(AlumniDomainErrorCodes.Membership.SubscriptionClosed);
            ex.WithData("SubscriptionFeeId", subscriptionFeeId);
            ex.WithData("FeeName", fee.Name);
            ex.WithData("SeasonEndDate", fee.SeasonEndDate);
            throw ex;
        }

        return fee;
    }

    /// <summary>
    /// Business Logic: Check for duplicate requests using idempotency key
    /// </summary>
    public async Task<AssociationRequest?> CheckIdempotencyAsync(string idempotencyKey)
    {
        Check.NotNullOrWhiteSpace(idempotencyKey, nameof(idempotencyKey));
        
        var list = await _requestRepository.GetListAsync(x => x.IdempotencyKey == idempotencyKey);
        return list.FirstOrDefault();
    }

    /// <summary>
    /// Business Logic: Calculate Validity Dates based on Grad Year ("Fresh Grad" Rule)
    /// </summary>
    public (DateTime StartDate, DateTime EndDate) CalculateValidityDates(int graduationYear, DateTime requestDate)
    {
        var currentYear = requestDate.Year;
        
        // Fresh Grad Logic: Request in Same Year OR Next Year
        if (currentYear == graduationYear || currentYear == graduationYear + 1)
        {
            // StartDate = Graduation Date (Assuming Jan 1st of Grad Year to cover full period, or July 1st?)
            // Let's use Jan 1st of Graduation Year as "Graduation Date" proxy if no specific date available
            var startDate = new DateTime(graduationYear, 1, 1); 
            
            // EndDate = End of the Next Year (Dec 31st of GradYear + 1)
            var endDate = new DateTime(graduationYear + 1, 12, 31);
            
            return (startDate, endDate);
        }
        
        // Standard Renewal Logic (Gap Years)
        // StartDate = Jan 1st of Current Year
        var stdStart = new DateTime(currentYear, 1, 1);
        var stdEnd = new DateTime(currentYear, 12, 31);
        
        return (stdStart, stdEnd);
    }

    /// <summary>
    /// Business Logic: Create new membership request with idempotency check
    /// </summary>
    public async Task<AssociationRequest> CreateMembershipRequestAsync(
        Guid requestId,
        Guid alumniId,
        Guid subscriptionFeeId,
        string idempotencyKey,
        Guid targetBranchId,
        int graduationYear,
        DeliveryMethod deliveryMethod,
        decimal deliveryFee,
        decimal usedWalletAmount,
        decimal remainingAmount,
        string? personalPhotoBlobName)
    {
        // Idempotency check
        var existing = await CheckIdempotencyAsync(idempotencyKey);
        if (existing != null)
        {
            return existing; // Return existing request (idempotent operation)
        }

        // Validate subscription fee
        await ValidateSubscriptionFeeAsync(subscriptionFeeId);

        // Calculate Validity Dates
        var validity = CalculateValidityDates(graduationYear, Clock.Now);

        // Create new request
        var request = new AssociationRequest(
            requestId,
            alumniId,
            subscriptionFeeId,
            idempotencyKey,
            targetBranchId,
            validity.StartDate,
            validity.EndDate,
            deliveryMethod,
            deliveryFee,
            usedWalletAmount,
            remainingAmount,
            personalPhotoBlobName
        );

        return request;
    }

    /// <summary>
    /// Business Logic: Validate payment before approving membership request
    /// CRITICAL: Financial Integrity Check
    /// </summary>
    public async Task ValidatePaymentForApprovalAsync(Guid requestId)
    {
        var paymentExists = await _paymentRepository.AnyAsync(
            x => x.RequestId == requestId && x.Status == PaymentStatus.Completed
        );

        if (!paymentExists)
        {
            throw new BusinessException(AlumniDomainErrorCodes.Membership.NoValidPayment)
                .WithData("RequestId", requestId);
        }
    }

    /// <summary>
    /// Business Logic: Approve membership request with payment validation
    /// </summary>
    public async Task ApproveRequestAsync(AssociationRequest request)
    {
        Check.NotNull(request, nameof(request));

        // Critical business rule: Cannot approve without valid payment
        await ValidatePaymentForApprovalAsync(request.Id);

        // Approve through domain entity method
        request.Approve();
    }

    /// <summary>
    /// Business Logic: Process payment for membership request
    /// </summary>
    public void ProcessPayment(AssociationRequest request, Guid currentUserId)
    {
        Check.NotNull(request, nameof(request));

        // Security: Resource ownership validation
        if (request.AlumniId != currentUserId)
        {
            throw new BusinessException(AlumniDomainErrorCodes.Membership.UnauthorizedPayment)
                .WithData("RequestId", request.Id)
                .WithData("RequestOwnerId", request.AlumniId)
                .WithData("CurrentUserId", currentUserId);
        }

        // Business rule: Can only pay pending requests
        if (request.Status != MembershipRequestStatus.Pending)
        {
            throw new BusinessException(AlumniDomainErrorCodes.Membership.RequestNotPayable)
                .WithData("RequestId", request.Id)
                .WithData("CurrentStatus", request.Status);
        }

        request.MarkAsPaid();
    }

    /// <summary>
    /// Business Logic: Check if alumni has active membership
    /// Rule #1: Membership Validity Check
    /// </summary>
    public async Task<bool> IsActiveAsync(Guid alumniId)
    {
        // Check if alumni has at least one approved membership request
        var hasApprovedRequest = await _requestRepository.AnyAsync(
            x => x.AlumniId == alumniId && x.Status == MembershipRequestStatus.Approved
        );
        
        return hasApprovedRequest;
    }

    /// <summary>
    /// Business Logic: Retrieve Card Print Data (Last Qualification Rule)
    /// </summary>
    public async Task<(AssociationRequest Request, Informatique.Alumni.Profiles.AlumniProfile Profile, Informatique.Alumni.Profiles.Education? Education)> GetCardDataAsync(Guid requestId)
    {
        var request = await _requestRepository.GetAsync(requestId);
        
        var profile = await _profileRepository.GetAsync(request.AlumniId);
        
        // Logic: Latest Qualification (Year Desc, Semester Desc)
        var education = profile.Educations
            .OrderByDescending(e => e.GraduationYear)
            .ThenByDescending(e => e.GraduationSemester)
            .FirstOrDefault();
            
        return (request, profile, education);
    }


    /// <summary>
    /// Business Logic: Calculate Renewal Fee based on "Gap" Period
    /// Rules: Gap <= 1 Year -> Tier 1, 1 < Gap <= 2 -> Tier 2, Gap > 2 -> Tier 3
    /// </summary>
    public async Task<decimal> CalculateRenewalFeeAsync(Guid alumniId)
    {
        // 1. Get Fee Config (Singleton Policy)
        var configs = await _feeConfigRepository.GetListAsync();
        var config = configs.FirstOrDefault();
        
        if (config == null)
        {
            throw new BusinessException("Alumni:Membership:FeeConfigMissing");
        }

        // 2. Get Last Approved Request to find Expiry Date
        // Using Repository directly. Assuming we can query by AlumniId
        var queryable = await _requestRepository.GetQueryableAsync();
        var lastRequest = queryable
            .Where(x => x.AlumniId == alumniId && x.Status == MembershipRequestStatus.Approved)
            .OrderByDescending(x => x.ValidityEndDate)
            .FirstOrDefault();

        if (lastRequest == null)
        {
            // First time or no history? 
            // If strictly "Renewal", this implies existing. If used for new, we might default to Tier 1.
            // Assuming "First Time" treats as Tier 1 per rules "Gap <= 1 Year (or First Time)".
            return config.OneYearFee;
        }

        // 3. Calculate Gap
        var gap = Clock.Now - lastRequest.ValidityEndDate;
        var totalDays = gap.TotalDays;
        
        // Logic
        // 1 Year ~ 365 Days. 2 Years ~ 730 Days.
        if (totalDays <= 365) // <= 1 Year
        {
            return config.OneYearFee;
        }
        else if (totalDays <= 730) // > 1 Year AND <= 2 Years
        {
            return config.TwoYearsFee;
        }
        else // > 2 Years
        {
            return config.MoreThanTwoYearsFee;
        }
    }
}
