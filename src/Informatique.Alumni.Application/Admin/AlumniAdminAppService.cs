using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Informatique.Alumni.Events;
using Informatique.Alumni.Career;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.AlumniManage)]
public class AlumniAdminAppService : AlumniAppService, IAlumniAdminAppService
{
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<Informatique.Alumni.Membership.AssociationRequest, Guid> _requestRepository;
    private readonly IRepository<Informatique.Alumni.Membership.SubscriptionFee, Guid> _feeRepository;
    private readonly IRepository<College, Guid> _collegeRepository;
    private readonly IRepository<Education, Guid> _educationRepository;
    private readonly IRepository<Informatique.Alumni.Events.AssociationEvent, Guid> _eventRepository;
    private readonly IRepository<Informatique.Alumni.Events.AlumniEventRegistration, Guid> _eventRegistrationRepository;
    private readonly IRepository<Informatique.Alumni.Career.Job, Guid> _jobRepository;
    private readonly IRepository<Informatique.Alumni.Career.JobApplication, Guid> _jobApplicationRepository;

    public AlumniAdminAppService(
        IRepository<AlumniProfile, Guid> profileRepository,
        IIdentityUserRepository userRepository,
        IdentityUserManager userManager,
        IRepository<Informatique.Alumni.Membership.AssociationRequest, Guid> requestRepository,
        IRepository<Informatique.Alumni.Membership.SubscriptionFee, Guid> feeRepository,
        IRepository<College, Guid> collegeRepository,
        IRepository<Education, Guid> educationRepository,
        IRepository<Informatique.Alumni.Events.AssociationEvent, Guid> eventRepository,
        IRepository<Informatique.Alumni.Events.AlumniEventRegistration, Guid> eventRegistrationRepository,
        IRepository<Informatique.Alumni.Career.Job, Guid> jobRepository,
        IRepository<Informatique.Alumni.Career.JobApplication, Guid> jobApplicationRepository)
    {
        _profileRepository = profileRepository;
        _userRepository = userRepository;
        _userManager = userManager;
        _requestRepository = requestRepository;
        _feeRepository = feeRepository;
        _collegeRepository = collegeRepository;
        _educationRepository = educationRepository;
        _eventRepository = eventRepository;
        _eventRegistrationRepository = eventRegistrationRepository;
        _jobRepository = jobRepository;
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<PagedResultDto<AlumniAdminListDto>> GetListAsync(AlumniAdminGetListInput input)
    {
        var queryable = await _profileRepository.GetQueryableAsync();

        if (input.StatusFilter.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.StatusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x =>
                x.MobileNumber.Contains(input.Filter) ||
                x.NationalId.Contains(input.Filter));
        }

        var totalCount = queryable.Count();

        queryable = queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var profiles = queryable.ToList();

        // Batch lookup user names
        var userIds = profiles.Select(p => p.UserId).ToList();
        var users = await _userRepository.GetListByIdsAsync(userIds);

        var items = profiles.Select(p =>
        {
            var user = users.FirstOrDefault(u => u.Id == p.UserId);
            return new AlumniAdminListDto
            {
                Id = p.Id,
                UserId = p.UserId,
                FullName = user != null ? $"{user.Name} {user.Surname}" : "—",
                Email = user?.Email ?? "—",
                MobileNumber = p.MobileNumber,
                Status = p.Status,
                IsVip = p.IsVip,
                IsNotable = p.IsNotable,
                IdCardStatus = p.IdCardStatus,
                CreationTime = p.CreationTime
            };
        }).ToList();

        return new PagedResultDto<AlumniAdminListDto>(totalCount, items);
    }

    public async Task<AlumniAdminDto> GetAsync(Guid id)
    {
        var profile = await _profileRepository.GetAsync(id);
        var user = (await _userRepository.GetListByIdsAsync(new[] { profile.UserId })).FirstOrDefault();

        return new AlumniAdminDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            UserName = user?.UserName ?? "—",
            Email = user?.Email ?? "—",
            FullName = user != null ? $"{user.Name} {user.Surname}" : "—",
            JobTitle = profile.JobTitle,
            Bio = profile.Bio,
            MobileNumber = profile.MobileNumber,
            NationalId = profile.NationalId,
            PhotoUrl = profile.PhotoUrl,
            Address = profile.Address,
            City = profile.City,
            Country = profile.Country,
            Company = profile.Company,
            FacebookUrl = profile.FacebookUrl,
            LinkedinUrl = profile.LinkedinUrl,
            Status = profile.Status,
            IsVip = profile.IsVip,
            IsNotable = profile.IsNotable,
            IdCardStatus = profile.IdCardStatus,
            RejectionReason = profile.RejectionReason,
            WalletBalance = profile.WalletBalance,
            ViewCount = profile.ViewCount,
            ShowInDirectory = profile.ShowInDirectory,
            CreationTime = profile.CreationTime,
            LastModificationTime = profile.LastModificationTime
        };
    }

    [Authorize(AlumniPermissions.Admin.AlumniApprove)]
    public async Task ApproveAsync(Guid id)
    {
        var profile = await _profileRepository.GetAsync(id);
        profile.Approve();
        await _profileRepository.UpdateAsync(profile);

        // Assign 'alumni' role if not present
        var user = await _userManager.GetByIdAsync(profile.UserId);
        if (user != null && !await _userManager.IsInRoleAsync(user, "alumni"))
        {
            await _userManager.AddToRoleAsync(user, "alumni");
        }
    }

    [Authorize(AlumniPermissions.Admin.AlumniManage)]
    public async Task<int> SyncMemberStatusAsync()
    {
        // 1. Get all Pending Profiles
        var pendingProfiles = await _profileRepository.GetListAsync(p => p.Status == Informatique.Alumni.Profiles.AlumniStatus.Pending);
        if (!pendingProfiles.Any()) return 0;

        int updatedCount = 0;
        
        // Ensure default fee exists for created requests
        var subscriptionFeeId = Guid.Empty;
        var fee = await _feeRepository.FirstOrDefaultAsync(x => x.Name == "Standard Membership");
        if (fee != null) subscriptionFeeId = fee.Id;

        // Batch fetch all related requests
        var profileIds = pendingProfiles.Select(p => p.Id).ToList();
        var requests = await _requestRepository.GetListAsync(r => profileIds.Contains(r.AlumniId));
        var requestsLookup = requests.ToLookup(r => r.AlumniId);

        // Batch fetch all users for role assignment
        var userIds = pendingProfiles.Select(p => p.UserId).Distinct().ToList();
        // Optimizing role checks: we can't easily batch "IsInRole" without identity repository access to UserRoles.
        // But we can minimize calls. 
        // Better: fetching users with roles is specific to Identity module. 
        // We will stick to EnsureRoleAsync per user for now as it's safer regarding IdentityManager internals, 
        // IF we optimize the loop logic. 
        // But we can at least load users into memory if needed. 
        // Actually, EnsureRoleAsync does Load + Check + Add.
        // Let's keep EnsureRoleAsync as is for safety, but we removed the N+1 on Requests which is the main data volume.
        
        foreach (var profile in pendingProfiles)
        {
            var profileRequests = requestsLookup[profile.Id].ToList();
            var hasApprovedRequest = profileRequests.Any(r => r.Status == Informatique.Alumni.Membership.MembershipRequestStatus.Approved);

            if (hasApprovedRequest)
            {
                // Case A: Mismatch (Profile Pending, Request Approved)
                profile.Approve();
                await _profileRepository.UpdateAsync(profile);
                await EnsureRoleAsync(profile.UserId);
                updatedCount++;
            }
            else if (!profileRequests.Any() && subscriptionFeeId != Guid.Empty)
            {
                // Case B: Orphan (Profile Pending, No Requests) - Likely Seeded Data
                // Auto-fix by creating an Approved Request
                var req = new Informatique.Alumni.Membership.AssociationRequest(
                    GuidGenerator.Create(),
                    profile.Id,
                    subscriptionFeeId,
                    Guid.NewGuid().ToString(),
                    profile.BranchId == Guid.Empty ? Guid.NewGuid() : profile.BranchId,
                    DateTime.UtcNow.AddDays(-1),
                    DateTime.UtcNow.AddYears(1),
                    Informatique.Alumni.Membership.DeliveryMethod.OfficePickup,
                    0, 0, 100, null
                );
                req.MarkAsPaid();
                req.Approve(); // Sets status to Approved
                
                await _requestRepository.InsertAsync(req);
                
                profile.Approve();
                await _profileRepository.UpdateAsync(profile);
                await EnsureRoleAsync(profile.UserId);
                updatedCount++;
            }
        }
        
        return updatedCount;
    }

    private async Task EnsureRoleAsync(Guid userId)
    {
        var user = await _userManager.GetByIdAsync(userId);
        if (user != null && !await _userManager.IsInRoleAsync(user, "alumni"))
        {
            await _userManager.AddToRoleAsync(user, "alumni");
        }
    }

    [Authorize(AlumniPermissions.Admin.AlumniApprove)]
    public async Task RejectAsync(Guid id, RejectAlumniInput input)
    {
        var profile = await _profileRepository.GetAsync(id);
        profile.Reject(input.Reason);
        await _profileRepository.UpdateAsync(profile);
    }

    [Authorize(AlumniPermissions.Admin.AlumniApprove)]
    public async Task BanAsync(Guid id)
    {
        var profile = await _profileRepository.GetAsync(id);
        profile.Ban();
        await _profileRepository.UpdateAsync(profile);
    }

    public async Task MarkAsNotableAsync(Guid id)
    {
        var profile = await _profileRepository.GetAsync(id);
        profile.MarkAsNotable(!profile.IsNotable); // Toggle
        await _profileRepository.UpdateAsync(profile);
    }

    public async Task UpdateIdCardStatusAsync(Guid id, UpdateIdCardStatusInput input)
    {
        var profile = await _profileRepository.GetAsync(id);
        profile.UpdateIdCardStatus(input.Status);
        await _profileRepository.UpdateAsync(profile);
    }

    [Authorize(AlumniPermissions.Admin.AlumniManage)]
    public async Task EnrichDataAsync()
    {
        var random = new Random();

        // 1. Ensure Colleges Exist
        var colleges = await _collegeRepository.GetListAsync();
        var collegeNames = new[] { "Faculty of Engineering", "Faculty of Business", "Faculty of Science", "Faculty of Medicine", "Faculty of Arts" };
        var createdColleges = new System.Collections.Generic.List<College>();

        foreach (var name in collegeNames)
        {
            var college = colleges.FirstOrDefault(c => c.Name == name);
            if (college == null)
            {
                college = new College(GuidGenerator.Create(), name, null, null);
                await _collegeRepository.InsertAsync(college);
            }
            createdColleges.Add(college);
        }

        // 2. Random Data Lists
        var companies = new[] { "Microsoft", "Google", "Amazon", "Vodafone", "CIB", "Valeo", "Orange", "Etisalat", "IBM", "Dell" };
        var cities = new[] { "Cairo", "Giza", "Alexandria", "Dubai", "London", "Munich", "Berlin", "Riyadh", "New York", "Paris" };
        var countries = new[] { "Egypt", "Egypt", "Egypt", "UAE", "UK", "Germany", "Germany", "KSA", "USA", "France" };
        var jobTitles = new[] { "Software Engineer", "Product Manager", "Data Scientist", "HR Specialist", "Accountant", "Doctor", "Architect", "Designer", "Consultant", "Lecturer" };

        // 3. Fetch all profiles
        // Note: For 230 profiles, fetching all is fine. For larger datasets, use batch processing.
        var profiles = await _profileRepository.GetListAsync();
        var educations = await _educationRepository.GetListAsync();
        var educationLookup = educations.ToLookup(e => e.AlumniProfileId);
        
        // Fetch Events and Jobs for Enrichment
        var events = await _eventRepository.GetListAsync();
        var jobs = await _jobRepository.GetListAsync();

        foreach (var profile in profiles)
        {
            // Randomize Professional Info
            var company = companies[random.Next(companies.Length)];
            var jobTitle = jobTitles[random.Next(jobTitles.Length)];
            profile.UpdateProfessionalInfo(company, jobTitle);

            // Randomize Location
            var cityIndex = random.Next(cities.Length);
            profile.UpdateAddress(profile.Address, cities[cityIndex], countries[cityIndex]);

            await _profileRepository.UpdateAsync(profile);

            // Randomize Education
            var profileEdu = educationLookup[profile.Id].FirstOrDefault();
            if (profileEdu != null)
            {
                 // Ensure valid college
                 var randomCollege = createdColleges[random.Next(createdColleges.Count)];
                 profileEdu.SetAcademicDetails(1, randomCollege.Id, null, null);
                 await _educationRepository.UpdateAsync(profileEdu);
            }
            else
            {
                // Create default education if missing
                var randomCollege = createdColleges[random.Next(createdColleges.Count)];
                var edu = new Education(GuidGenerator.Create(), profile.Id, "Cairo University", "Bachelor", 2020);
                edu.SetAcademicDetails(1, randomCollege.Id, null, null);
                await _educationRepository.InsertAsync(edu);
            }
        }
    }
}
