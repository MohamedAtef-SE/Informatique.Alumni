using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Guidance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Informatique.Alumni.Branches;

using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Volo.Abp.BlobStoring;
using Informatique.Alumni.Payment;
using Informatique.Alumni.Events;

namespace Informatique.Alumni.Profiles;

[Authorize]
public class AlumniProfileAppService : AlumniAppService, IAlumniProfileAppService
{
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IStudentSystemIntegrationService _studentSystemIntegration;
    private readonly AlumniApplicationMappers _alumniMappers;
    private readonly IBlobContainer<ProfilePictureContainer> _blobContainer;
    private readonly Volo.Abp.Identity.IdentityUserManager _userManager;
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IRepository<Major, Guid> _majorRepository;
    private readonly IRepository<College, Guid> _collegeRepository;
    private readonly IRepository<PaymentTransaction, Guid> _paymentRepository;
    private readonly IRepository<AlumniEventRegistration, Guid> _registrationRepository;
    private readonly IRepository<AssociationEvent, Guid> _eventRepository;

    public AlumniProfileAppService(
        IRepository<AlumniProfile, Guid> profileRepository,
        IStudentSystemIntegrationService studentSystemIntegration,
        AlumniApplicationMappers alumniMappers,
        IBlobContainer<ProfilePictureContainer> blobContainer,
        Volo.Abp.Identity.IdentityUserManager userManager,
        IRepository<Branch, Guid> branchRepository,
        IRepository<Major, Guid> majorRepository,
        IRepository<College, Guid> collegeRepository,
        IRepository<PaymentTransaction, Guid> paymentRepository,
        IRepository<AlumniEventRegistration, Guid> registrationRepository,
        IRepository<AssociationEvent, Guid> eventRepository)
    {
        _profileRepository = profileRepository;
        _studentSystemIntegration = studentSystemIntegration;
        _alumniMappers = alumniMappers;
        _blobContainer = blobContainer;
        _userManager = userManager;
        _branchRepository = branchRepository;
        _majorRepository = majorRepository;
        _collegeRepository = collegeRepository;
        _paymentRepository = paymentRepository;
        _registrationRepository = registrationRepository;
        _eventRepository = eventRepository;
    }

    public async Task<AlumniMyProfileDto> GetMyProfileAsync()
    {
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId());
        var dto = _alumniMappers.MapToDto(profile);

        var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
        if (user != null)
        {
            dto.Name = $"{user.Name} {user.Surname}".Trim();
            dto.NameAr = string.IsNullOrEmpty(user.Name) ? dto.Name : user.Name;
            dto.NameEn = string.IsNullOrEmpty(user.Name) ? dto.Name : user.Name;
        }

        var branches = await _branchRepository.GetListAsync();
        var majors = await _majorRepository.GetListAsync();
        var colleges = await _collegeRepository.GetListAsync();

        dto.Educations = profile.Educations.Select(e => new AlumniEducationDto
        {
            Id = e.Id,
            InstitutionName = e.InstitutionName,
            Degree = e.Degree,
            GraduationYear = e.GraduationYear,
            GraduationSemester = e.GraduationSemester,
            College = colleges.FirstOrDefault(c => c.Id == e.CollegeId)?.Name ?? "N/A",
            Major = majors.FirstOrDefault(m => m.Id == e.MajorId)?.Name ?? "N/A"
        }).ToList();

        return dto;
    }

    public async Task<AlumniMyProfileDto> UpdateMyProfileAsync(UpdateMyProfileDto input)
    {
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId());
        
        // Use domain methods for direct updates
        profile.UpdateBasicInfo(profile.MobileNumber, input.Bio, input.JobTitle); 
        profile.UpdateAddress(input.Address, input.City, input.Country);
        profile.UpdateProfessionalInfo(input.Company, input.JobTitle);
        profile.UpdateSocialLinks(input.FacebookUrl, input.LinkedinUrl);
        
        await _profileRepository.UpdateAsync(profile);
        return _alumniMappers.MapToDto(profile);
    }

    public async Task<string> UploadPhotoAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new UserFriendlyException("File is empty.");
        }

        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId());
        var blobName = $"profiles/{profile.Id}_{GuidGenerator.Create()}.png";

        using (var stream = file.OpenReadStream())
        {
            await _blobContainer.SaveAsync(blobName, stream, overrideExisting: true);
        }

        profile.SetPhotoUrl(blobName);
        await _profileRepository.UpdateAsync(profile);

        return blobName;
    }

    public async Task ApplyAsAdvisorAsync(ApplyAsAdvisorDto input)
    {
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId());
        profile.ApplyForAdvisory(input.Bio, input.ExperienceYears, input.ExpertiseIds);
        await _profileRepository.UpdateAsync(profile);
    }

    public async Task<AdvisoryStatusDto> GetAdvisoryStatusAsync()
    {
        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId());
        return new AdvisoryStatusDto
        {
            Status = profile.AdvisoryStatus,
            Bio = profile.AdvisoryBio,
            ExperienceYears = profile.AdvisoryExperienceYears,
            RejectionReason = profile.AdvisoryRejectionReason,
            ExpertiseIds = profile.AdvisoryExpertises.Select(x => x.AdvisoryCategoryId).ToList()
        };
    }

    public async Task<AlumniMyProfileDto> TopUpWalletAsync(decimal amount)
    {
        if (amount <= 0)
        {
            throw new UserFriendlyException("Top-up amount must be greater than zero.");
        }

        var profile = await GetOrCreateProfileAsync(CurrentUser.GetId());
        
        // 1. Update Profile Balance
        profile.AddCredit(amount);
        await _profileRepository.UpdateAsync(profile);

        // 2. Create Transaction Ledger Entry
        var transaction = new PaymentTransaction(
            id: GuidGenerator.Create(),
            orderId: profile.Id, // Linking to profile for now
            amount: amount,
            currency: "EGP",
            gatewayTransactionId: "TXN-" + GuidGenerator.Create().ToString("N").ToUpper().Substring(0, 10)
        );
        transaction.MarkAsCompleted(transaction.GatewayTransactionId);
        
        await _paymentRepository.InsertAsync(transaction);
        
        return await GetMyProfileAsync();
    }

    public async Task<List<WalletActivityDto>> GetWalletActivityAsync()
    {
        var alumniId = (await GetOrCreateProfileAsync(CurrentUser.GetId())).Id;
        var activity = new List<WalletActivityDto>();

        // 1. Get Top-ups (Deposits)
        var transactions = await _paymentRepository.GetListAsync(x => x.OrderId == alumniId && x.Status == PaymentStatus.Completed);
        activity.AddRange(transactions.Select(x => new WalletActivityDto
        {
            Id = x.Id,
            Amount = x.Amount,
            Description = "Wallet Top-up",
            TransactionDate = x.CreationTime,
            Type = "Deposit"
        }));

        // 2. Get Event Payments (Withdrawals)
        var registrations = await _registrationRepository.GetListAsync(x => x.AlumniId == alumniId && x.PaidAmount > 0);
        if (registrations.Any())
        {
            var eventIds = registrations.Select(r => r.EventId).Distinct().ToList();
            var events = await _eventRepository.GetListAsync(x => eventIds.Contains(x.Id));

            activity.AddRange(registrations.Select(x => new WalletActivityDto
            {
                Id = x.Id,
                Amount = x.PaidAmount ?? 0,
                Description = events.FirstOrDefault(e => e.Id == x.EventId)?.NameEn ?? "Event Registration",
                TransactionDate = x.CreationTime,
                Type = "Payment"
            }));
        }

        return activity.OrderByDescending(x => x.TransactionDate).Take(10).ToList();
    }

    private async Task<AlumniProfile> GetOrCreateProfileAsync(Guid userId)
    {
        var profileQuery = await _profileRepository.WithDetailsAsync(
            x => x.Educations,
            x => x.Experiences,
            x => x.Emails,
            x => x.Mobiles,
            x => x.Phones
        );

        var profile = profileQuery.FirstOrDefault(x => x.UserId == userId);
        if (profile == null)
        {
            throw new UserFriendlyException("Profile not found.");
        }
        return profile;
    }
}
