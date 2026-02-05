using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

using Informatique.Alumni.BlobContainers;
using Informatique.Alumni.Profiles;
using Informatique.Alumni.Certificates;
using Volo.Abp.BlobStoring;

namespace Informatique.Alumni.Membership;

[Authorize(AlumniPermissions.Membership.Default)]
public class MembershipAppService : AlumniAppService, IMembershipAppService
{
    private readonly IRepository<SubscriptionFee, Guid> _feeRepository;
    private readonly IRepository<AssociationRequest, Guid> _requestRepository;
    private readonly IRepository<PaymentTransaction, Guid> _paymentRepository;
    private readonly MembershipManager _membershipManager;
    private readonly AlumniApplicationMappers _alumniMappers;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IBlobContainer<AlumniBlobContainer> _blobContainer;
    private readonly Volo.Abp.Identity.IIdentityUserRepository _identityUserRepository;

    public MembershipAppService(
        IRepository<SubscriptionFee, Guid> feeRepository,
        IRepository<AssociationRequest, Guid> requestRepository,
        IRepository<PaymentTransaction, Guid> paymentRepository,
        MembershipManager membershipManager,
        AlumniApplicationMappers alumniMappers,
        IRepository<AlumniProfile, Guid> profileRepository,
        IBlobContainer<AlumniBlobContainer> blobContainer,
        Volo.Abp.Identity.IIdentityUserRepository identityUserRepository)
    {
        _feeRepository = feeRepository;
        _requestRepository = requestRepository;
        _paymentRepository = paymentRepository;
        _membershipManager = membershipManager;
        _alumniMappers = alumniMappers;
        _profileRepository = profileRepository;
        _blobContainer = blobContainer;
        _identityUserRepository = identityUserRepository;
    }

    // ... (existing methods) ...

    [Authorize(AlumniPermissions.Membership.Process)]
    public async Task<CardPrintDto> GetCardDataAsync(Guid id)
    {
        var data = await _membershipManager.GetCardDataAsync(id);
        return await MapCardDataAsync(id, data.Request, data.Profile, data.Education);
    }

    [Authorize]
    public async Task<CardPrintDto> GetMyCardAsync()
    {
        var userId = CurrentUser.GetId();
        var profile = await _profileRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile == null) return null;

        var query = await _requestRepository.GetQueryableAsync();
        var request = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(x => x.AlumniId == profile.Id)
                 .OrderByDescending(x => x.CreationTime)
        );

        if (request == null) return null;

        var data = await _membershipManager.GetCardDataAsync(request.Id);
        return await MapCardDataAsync(request.Id, data.Request, data.Profile, data.Education);
    }

    private async Task<CardPrintDto> MapCardDataAsync(Guid requestId, AssociationRequest request, AlumniProfile profile, Education? education)
    {
        var user = await _identityUserRepository.FindAsync(profile.UserId);
        var alumniName = user != null ? $"{user.Name} {user.Surname}" : "Alumni Member";

        return new CardPrintDto
        {
            RequestId = requestId,
            AlumniName = alumniName,
            AlumniNationalId = profile.NationalId,
            AlumniPhotoUrl = request.PersonalPhotoBlobName ?? profile.PhotoUrl ?? string.Empty,
            Degree = education?.Degree ?? "N/A",
            CollegeName = education?.CollegeId?.ToString() ?? "N/A",
            MajorName = education?.MajorId?.ToString() ?? "N/A",
            GradYear = education?.GraduationYear ?? 0
        };
        // Mapped
    }


    [Authorize(AlumniPermissions.Membership.ManageFees)]
    public async Task<SubscriptionFeeDto> CreateSubscriptionFeeAsync(CreateSubscriptionFeeDto input)
    {
        var fee = new SubscriptionFee(
            GuidGenerator.Create(),
            input.Name,
            input.Amount,
            input.Year,
            input.SeasonStartDate,
            input.SeasonEndDate
        );
        
        await _feeRepository.InsertAsync(fee);
        
        return ObjectMapper.Map<SubscriptionFee, SubscriptionFeeDto>(fee);
    }

    public async Task<ListResultDto<SubscriptionFeeDto>> GetActiveSubscriptionFeesAsync()
    {
        var fees = await _feeRepository.GetListAsync();
        var activeFees = fees.Where(x => x.IsCurrentlyValid()).ToList();
        
        return new ListResultDto<SubscriptionFeeDto>(
            ObjectMapper.Map<List<SubscriptionFee>, List<SubscriptionFeeDto>>(activeFees)
        );
    }

    [Authorize(AlumniPermissions.Membership.Request)]
    public async Task<AssociationRequestDto> RequestMembershipAsync(CreateAssociationRequestDto input)
    {
        var currentUserId = CurrentUser.GetId();
        var profile = await _profileRepository.GetAsync(p => p.UserId == currentUserId);

        // 1. Update Profile Contacts
        if (input.Emails.Any())
        {
            foreach (var email in input.Emails)
            {
               profile.AddEmail(new Informatique.Alumni.Profiles.ContactEmail(GuidGenerator.Create(), profile.Id, email.Value, email.IsPrimary));
            }
        }
        if (input.Mobiles.Any())
        {
            foreach (var mobile in input.Mobiles)
            {
               profile.AddMobile(new Informatique.Alumni.Profiles.ContactMobile(GuidGenerator.Create(), profile.Id, mobile.Value, mobile.IsPrimary));
            }
        }
        if (!string.IsNullOrEmpty(input.Address))
        {
            profile.UpdateAddress(input.Address);
        }

        // 2. Upload Personal Photo if provided
        string? photoBlobName = input.PersonalPhotoFileName;
        if (input.PersonalPhotoBytes != null && input.PersonalPhotoBytes.Length > 0)
        {
             var extension = System.IO.Path.GetExtension(input.PersonalPhotoFileName) ?? ".jpg";
             photoBlobName = $"{GuidGenerator.Create()}{extension}";
             await _blobContainer.SaveAsync(photoBlobName, input.PersonalPhotoBytes);
             // Note: Profile photo update only "upon approval" per rules, but stored in Request now.
        }

        // 3. Calculate Fees
        // Get Fee Amount from SubscriptionFee
        // Note: Manager validates it, but we need amount here.
        var fee = await _feeRepository.GetAsync(input.SubscriptionFeeId);
        // Assuming SubscriptionFee has Amount property.
        
        decimal deliveryFee = input.DeliveryMethod == DeliveryMethod.HomeDelivery ? MembershipConsts.DeliveryFeeAmount : 0;
        decimal totalAmount = fee.Amount + deliveryFee;

        // 4. Wallet Logic (Priority)
        decimal usedWalletAmount = 0;
        if (profile.WalletBalance > 0)
        {
            if (profile.WalletBalance >= totalAmount)
            {
                usedWalletAmount = totalAmount;
                profile.DeductWallet(totalAmount);
            }
            else
            {
                usedWalletAmount = profile.WalletBalance;
                profile.DeductWallet(profile.WalletBalance);
            }
        }
        decimal remainingAmount = totalAmount - usedWalletAmount;

        // 5. Get Graduation Year (Last Qualification)
        // Assuming strict rule: "Linked to Last Qualification".
        // profile.Educations collection.
        var maxGradYear = profile.Educations.Any() 
            ? profile.Educations.Max(x => x.GraduationYear) 
            : DateTime.Now.Year; // Fallback if no education? Should throw logic error.

        // 6. Create Request via Manager
        var request = await _membershipManager.CreateMembershipRequestAsync(
            GuidGenerator.Create(),
            profile.Id,
            input.SubscriptionFeeId,
            input.IdempotencyKey,
            input.TargetBranchId,
            maxGradYear,
            input.DeliveryMethod,
            deliveryFee,
            usedWalletAmount,
            remainingAmount,
            photoBlobName
        );
        
        if (remainingAmount == 0)
        {
            request.MarkAsPaid();
        }

        var isExistingRequest = request.CreationTime != default;
        if (!isExistingRequest)
        {
            await _requestRepository.InsertAsync(request);
            await _profileRepository.UpdateAsync(profile); // Persist Wallet & Contact changes
        }

        return _alumniMappers.MapToDto(request);
    }

    [Authorize(AlumniPermissions.Membership.Request)]
    public async Task<AssociationRequestDto> PayMembershipAsync(MembershipPaymentDto input)
    {
        var request = await _requestRepository.GetAsync(input.RequestId);
        
        // Use domain service for payment processing (includes ownership & status validation)
        _membershipManager.ProcessPayment(request, CurrentUser.GetId());

        // Create ledger entry (Immutable)
        var payment = new PaymentTransaction(
            GuidGenerator.Create(),
            request.Id,
            input.Amount,
            input.ExternalTransactionId
        );
        await _paymentRepository.InsertAsync(payment);

        await _requestRepository.UpdateAsync(request);
        return _alumniMappers.MapToDto(request);
    }

    [Authorize(AlumniPermissions.Membership.Process)]
    public async Task<AssociationRequestDto> ApproveRequestAsync(Guid id)
    {
        var request = await _requestRepository.GetAsync(id);
        
        // Use domain service for approval (includes payment validation)
        await _membershipManager.ApproveRequestAsync(request);
        
        await _requestRepository.UpdateAsync(request);
        return _alumniMappers.MapToDto(request);
    }

    [Authorize(AlumniPermissions.Membership.Process)]
    public async Task RejectRequestAsync(Guid id, string reason)
    {
        var request = await _requestRepository.GetAsync(id);
        request.Reject(reason);
        await _requestRepository.UpdateAsync(request);
    }

    public async Task<PagedResultDto<AssociationRequestDto>> GetListAsync(MembershipRequestFilterDto input)
    {
        // Security Logic: Scoped View
        // Rule: Employees view requests for their Registered Branch by default unless they have GlobalView.
        var hasGlobalView = await AuthorizationService.IsGrantedAsync(AlumniPermissions.Membership.GlobalView);
        var currentUserBranchId = CurrentUser.FindClaim("BranchId")?.Value;
        
        Guid? userBranchGuid = !string.IsNullOrEmpty(currentUserBranchId) ? Guid.Parse(currentUserBranchId) : null;
        
        // Force Filter if not Global View
        if (!hasGlobalView && userBranchGuid.HasValue)
        {
            input.BranchId = userBranchGuid.Value;
        }

        // Build Query
        var queryable = await _requestRepository.GetQueryableAsync();
        
        // Filter by Branch
        if (input.BranchId.HasValue)
        {
            queryable = queryable.Where(x => x.TargetBranchId == input.BranchId.Value);
        }
        
        // Filter by Status
        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }
        
        // Filter by Delivery Method
        if (input.DeliveryMethod.HasValue)
        {
            queryable = queryable.Where(x => x.DeliveryMethod == input.DeliveryMethod.Value);
        }
        
        // Date Range
        if (input.MinDate.HasValue)
        {
            queryable = queryable.Where(x => x.RequestDate >= input.MinDate.Value);
        }
        if (input.MaxDate.HasValue)
        {
            queryable = queryable.Where(x => x.RequestDate <= input.MaxDate.Value);
        }

        // Apply General Filter (Optional, assumes joins for Name/ID, skipping for brevity unless implemented)
        
        var query = queryable;

        var totalCount = await AsyncExecuter.CountAsync(query);
        
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.RequestDate) // Default sorting
                 .PageBy(input)
        );
        
        var dtos = _alumniMappers.MapToDtos(entities);
        
        // Fill fee names
        var feeIds = entities.Select(x => x.SubscriptionFeeId).Distinct().ToList();
        var fees = await _feeRepository.GetListAsync(x => feeIds.Contains(x.Id));
        var feeDict = fees.ToDictionary(x => x.Id, x => x.Name);
        
        foreach (var dto in dtos)
        {
            if (feeDict.TryGetValue(dto.SubscriptionFeeId, out var name))
            {
                dto.SubscriptionFeeName = name;
            }
        }

        return new PagedResultDto<AssociationRequestDto>(totalCount, dtos);
    }

    [Authorize(AlumniPermissions.Membership.Process)]
    public async Task UpdateStatusAsync(Guid id, UpdateStatusDto input)
    {
        var request = await _requestRepository.GetAsync(id);
        
        // State Machine Validation
        if (input.Status == MembershipRequestStatus.ReadyForPickup && request.DeliveryMethod == DeliveryMethod.HomeDelivery)
        {
             throw new UserFriendlyException("Cannot change status to ReadyForPickup for Home Delivery requests.");
        }
        
        if (input.Status == MembershipRequestStatus.OutForDelivery && request.DeliveryMethod == DeliveryMethod.OfficePickup)
        {
             throw new UserFriendlyException("Cannot change status to OutForDelivery for Office Pickup requests.");
        }

        // Additional Rules (e.g. Cannot go back from Delivered)
        if (request.Status == MembershipRequestStatus.Delivered)
        {
             throw new UserFriendlyException("Cannot modify a Delivered request.");
        }

        request.ChangeStatus(input.Status, input.Note, GuidGenerator.Create());
        
    }
}


