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
        
        // Build name with fallback: Name+Surname -> UserName -> Email prefix -> Default
        string alumniName = "Alumni Member";
        if (user != null)
        {
            if (!string.IsNullOrWhiteSpace(user.Name) || !string.IsNullOrWhiteSpace(user.Surname))
            {
                alumniName = $"{user.Name} {user.Surname}".Trim();
            }
            else if (!string.IsNullOrWhiteSpace(user.UserName))
            {
                // Use username as display name
                alumniName = user.UserName.Replace("_", " ").Replace(".", " ");
                // Capitalize first letters
                alumniName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(alumniName);
            }
            else if (!string.IsNullOrWhiteSpace(user.Email))
            {
                alumniName = user.Email.Split('@')[0];
            }

        }
        
        var photoUrl = request.PersonalPhotoBlobName ?? profile.PhotoUrl;
        
        // Fix for legacy URLs (migrating from /api/app/alumni-profile/photo/ to /api/profile-photo/)
        if (!string.IsNullOrEmpty(photoUrl) && photoUrl.Contains("/api/app/alumni-profile/photo/"))
        {
             photoUrl = photoUrl.Replace("/api/app/alumni-profile/photo/", "/api/profile-photo/");
        }

        if (!string.IsNullOrEmpty(photoUrl) && 
            !photoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) && 
            !photoUrl.StartsWith("/"))
        {
            // Assuming we have an endpoint to serve profile photos by blob name
            // Adjust the path if necessary based on your API
            photoUrl = $"/api/profile-photo/{photoUrl}";
        }

        return new CardPrintDto
        {
            RequestId = requestId,
            AlumniName = alumniName,
            AlumniNationalId = profile.NationalId,
            AlumniPhotoUrl = photoUrl ?? string.Empty,
            Degree = education?.Degree ?? "N/A",
            CollegeName = education?.InstitutionName ?? "N/A",
            MajorName = education?.Degree ?? "N/A", // Fallback to Degree if MajorId unavailable
            GradYear = education?.GraduationYear ?? 0,
            IsActive = request.Status >= MembershipRequestStatus.Paid
        };
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
        var hasGlobalView = await AuthorizationService.IsGrantedAsync(AlumniPermissions.Membership.GlobalView);
        var currentUserBranchId = CurrentUser.FindClaim("BranchId")?.Value;
        
        Guid? userBranchGuid = !string.IsNullOrEmpty(currentUserBranchId) ? Guid.Parse(currentUserBranchId) : null;
        
        if (!hasGlobalView && userBranchGuid.HasValue)
        {
            input.BranchId = userBranchGuid.Value;
        }

        // Build Query
        var queryable = await _requestRepository.GetQueryableAsync();
        
        if (input.BranchId.HasValue)
            queryable = queryable.Where(x => x.TargetBranchId == input.BranchId.Value);
        if (input.Status.HasValue)
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        if (input.DeliveryMethod.HasValue)
            queryable = queryable.Where(x => x.DeliveryMethod == input.DeliveryMethod.Value);
        if (input.MinDate.HasValue)
            queryable = queryable.Where(x => x.RequestDate >= input.MinDate.Value);
        if (input.MaxDate.HasValue)
            queryable = queryable.Where(x => x.RequestDate <= input.MaxDate.Value);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        
        var entities = await AsyncExecuter.ToListAsync(
            queryable.OrderByDescending(x => x.RequestDate).PageBy(input)
        );
        
        var dtos = _alumniMappers.MapToDtos(entities);

        // ── Batch Load: Fees ──
        var feeIds = entities.Select(x => x.SubscriptionFeeId).Distinct().ToList();
        var fees = await _feeRepository.GetListAsync(x => feeIds.Contains(x.Id));
        var feeDict = fees.ToDictionary(x => x.Id);

        // ── Batch Load: Alumni Profiles (with Educations) ──
        var alumniIds = entities.Select(x => x.AlumniId).Distinct().ToList();
        var profileQuery = await _profileRepository.WithDetailsAsync(p => p.Educations);
        var profiles = await AsyncExecuter.ToListAsync(
            profileQuery.Where(p => alumniIds.Contains(p.Id))
        );
        var profileDict = profiles.ToDictionary(p => p.Id);

        // ── Batch Load: Identity Users ──
        var userIds = profiles.Select(p => p.UserId).Distinct().ToList();
        var users = await _identityUserRepository.GetListByIdsAsync(userIds);
        var userDict = users.ToDictionary(u => u.Id);

        // ── Batch Load: Payments (check existence per request) ──
        var requestIds = entities.Select(x => x.Id).ToList();
        var paidRequestIds = (await _paymentRepository.GetListAsync(
            p => requestIds.Contains(p.RequestId) && p.Status == PaymentStatus.Completed
        )).Select(p => p.RequestId).ToHashSet();

        // ── Batch Load: Already-approved alumni (for duplicate check) ──
        var approvedAlumniIds = (await AsyncExecuter.ToListAsync(
            (await _requestRepository.GetQueryableAsync())
                .Where(r => alumniIds.Contains(r.AlumniId) 
                         && r.Status == MembershipRequestStatus.Approved)
                .Select(r => new { r.AlumniId, r.Id })
        )).GroupBy(r => r.AlumniId)
          .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToHashSet());

        // ── Populate DTOs ──
        for (int i = 0; i < dtos.Count; i++)
        {
            var dto = dtos[i];
            var entity = entities[i];

            // Fee name
            dto.SubscriptionFeeName = feeDict.TryGetValue(dto.SubscriptionFeeId, out var fee)
                ? fee.Name : "Unknown Fee";

            // Alumni identity
            var profile = profileDict.GetValueOrDefault(entity.AlumniId);
            var user = profile != null ? userDict.GetValueOrDefault(profile.UserId) : null;

            dto.AlumniName = user != null ? $"{user.Name} {user.Surname}" : "Unknown Alumni";
            dto.AlumniNationalId = profile?.NationalId ?? "—";
            dto.AlumniPhotoUrl = profile?.PhotoUrl;

            // Education (latest)
            var education = profile?.Educations
                .OrderByDescending(e => e.GraduationYear)
                .ThenByDescending(e => e.GraduationSemester)
                .FirstOrDefault();
            dto.CollegeName = education?.InstitutionName;
            dto.GraduationYear = education?.GraduationYear;

            // ── Run 6 Eligibility Checks ──
            dto.EligibilityChecks = BuildEligibilityChecks(
                entity, profile, education, fee, paidRequestIds, approvedAlumniIds);

            dto.EligibilitySummary = dto.EligibilityChecks.Any(c => c.Status == "Fail")
                ? "CannotApprove"
                : dto.EligibilityChecks.Any(c => c.Status == "Warning")
                    ? "NeedsReview"
                    : "AllClear";
        }

        return new PagedResultDto<AssociationRequestDto>(totalCount, dtos);
    }

    /// <summary>
    /// Runs 6 business-rule eligibility checks for a single membership request.
    /// </summary>
    private static List<EligibilityCheckDto> BuildEligibilityChecks(
        AssociationRequest entity,
        AlumniProfile? profile,
        Education? education,
        SubscriptionFee? fee,
        HashSet<Guid> paidRequestIds,
        Dictionary<Guid, HashSet<Guid>> approvedAlumniIds)
    {
        var checks = new List<EligibilityCheckDto>();

        // 1. Payment Check
        if (entity.Status == MembershipRequestStatus.Pending)
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Payment",
                Status = "Fail",
                Message = "No payment received yet. Alumni must pay before approval.",
                Icon = "x"
            });
        }
        else if (paidRequestIds.Contains(entity.Id) || entity.Status >= MembershipRequestStatus.Paid)
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Payment",
                Status = "Pass",
                Message = "Payment verified.",
                Icon = "check"
            });
        }
        else
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Payment",
                Status = "Fail",
                Message = "No completed payment found for this request.",
                Icon = "x"
            });
        }

        // 2. Duplicate Check
        if (approvedAlumniIds.TryGetValue(entity.AlumniId, out var approvedIds)
            && approvedIds.Any(id => id != entity.Id))
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Duplicate",
                Status = "Fail",
                Message = "This alumni already has another approved membership request.",
                Icon = "x"
            });
        }
        else
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Duplicate",
                Status = "Pass",
                Message = "No duplicate active membership found.",
                Icon = "check"
            });
        }

        // 3. Profile Completeness
        if (profile == null)
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Profile",
                Status = "Fail",
                Message = "Alumni profile not found.",
                Icon = "x"
            });
        }
        else if (string.IsNullOrWhiteSpace(profile.NationalId))
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Profile",
                Status = "Fail",
                Message = "Missing National ID — required for membership card.",
                Icon = "x"
            });
        }
        else if (string.IsNullOrWhiteSpace(profile.PhotoUrl))
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Profile",
                Status = "Warning",
                Message = "No profile photo uploaded. Recommended for ID card.",
                Icon = "alert-triangle"
            });
        }
        else
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Profile",
                Status = "Pass",
                Message = "Profile complete (National ID + Photo).",
                Icon = "check"
            });
        }

        // 4. Graduation / Education Check
        if (education == null)
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Graduation",
                Status = "Fail",
                Message = "No education records found. Cannot verify alumni status.",
                Icon = "x"
            });
        }
        else if (string.IsNullOrWhiteSpace(education.InstitutionName))
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Graduation",
                Status = "Warning",
                Message = $"Education record exists (Year: {education.GraduationYear}) but institution name is missing.",
                Icon = "alert-triangle"
            });
        }
        else
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Graduation",
                Status = "Pass",
                Message = $"Verified: {education.InstitutionName}, Class of {education.GraduationYear}.",
                Icon = "check"
            });
        }

        // 5. Alumni Status Check
        if (profile != null)
        {
            if (profile.Status == AlumniStatus.Banned)
            {
                checks.Add(new EligibilityCheckDto
                {
                    CheckName = "Alumni Status",
                    Status = "Fail",
                    Message = "This alumni is BANNED. Cannot approve membership.",
                    Icon = "x"
                });
            }
            else if (profile.Status == AlumniStatus.Rejected)
            {
                checks.Add(new EligibilityCheckDto
                {
                    CheckName = "Alumni Status",
                    Status = "Fail",
                    Message = "This alumni profile was previously rejected.",
                    Icon = "x"
                });
            }
            else
            {
                checks.Add(new EligibilityCheckDto
                {
                    CheckName = "Alumni Status",
                    Status = "Pass",
                    Message = $"Alumni status: {profile.Status}.",
                    Icon = "check"
                });
            }
        }
        else
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Alumni Status",
                Status = "Fail",
                Message = "Alumni profile not found.",
                Icon = "x"
            });
        }

        // 6. Fee Validity Check
        if (fee == null)
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Fee Validity",
                Status = "Fail",
                Message = "Subscription fee record not found.",
                Icon = "x"
            });
        }
        else if (!fee.IsCurrentlyValid())
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Fee Validity",
                Status = "Warning",
                Message = $"Subscription fee \"{fee.Name}\" season ended on {fee.SeasonEndDate:yyyy-MM-dd}.",
                Icon = "alert-triangle"
            });
        }
        else
        {
            checks.Add(new EligibilityCheckDto
            {
                CheckName = "Fee Validity",
                Status = "Pass",
                Message = $"Fee \"{fee.Name}\" is valid until {fee.SeasonEndDate:yyyy-MM-dd}.",
                Icon = "check"
            });
        }

        return checks;
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


