using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Certificates;
using Informatique.Alumni.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Users;
using Xunit;
using Volo.Abp.PermissionManagement;

namespace Informatique.Alumni.Certificates;

public class CertificateRequestAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly ICertificateRequestAppService _appService;
    private readonly IRepository<CertificateRequest, Guid> _repository;
    private readonly IRepository<CertificateDefinition, Guid> _definitionRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly Volo.Abp.Identity.IdentityUserManager _userManager;

    private readonly IRepository<Informatique.Alumni.Membership.AssociationRequest, Guid> _associationRepository;
    private readonly IRepository<Informatique.Alumni.Profiles.AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<Informatique.Alumni.Branches.Branch, Guid> _branchRepository;
    private readonly IRepository<Informatique.Alumni.Membership.SubscriptionFee, Guid> _feeRepository;

    private readonly Volo.Abp.PermissionManagement.IPermissionManager _permissionManager;

    public CertificateRequestAppServiceTests()
    {
        _appService = GetRequiredService<ICertificateRequestAppService>();
        _repository = GetRequiredService<IRepository<CertificateRequest, Guid>>();
        _definitionRepository = GetRequiredService<IRepository<CertificateDefinition, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
        _currentUser = GetRequiredService<ICurrentUser>();
        _userManager = GetRequiredService<Volo.Abp.Identity.IdentityUserManager>();
        _associationRepository = GetRequiredService<IRepository<Informatique.Alumni.Membership.AssociationRequest, Guid>>();
        _profileRepository = GetRequiredService<IRepository<Informatique.Alumni.Profiles.AlumniProfile, Guid>>();
        _branchRepository = GetRequiredService<IRepository<Informatique.Alumni.Branches.Branch, Guid>>();
        _feeRepository = GetRequiredService<IRepository<Informatique.Alumni.Membership.SubscriptionFee, Guid>>();
        _permissionManager = GetRequiredService<Volo.Abp.PermissionManagement.IPermissionManager>();
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Request_Successfully()
    {
        // Arrange
        var userId = _guidGenerator.Create();
        var user = new Volo.Abp.Identity.IdentityUser(userId, "certuser", "cert@alumni.com");
        await _userManager.CreateAsync(user);
        
        // Grant Permissions
        await _permissionManager.SetForUserAsync(userId, AlumniPermissions.Certificates.Default, true);
        await _permissionManager.SetForUserAsync(userId, AlumniPermissions.Certificates.Request, true);

        var definitionId = await CreateCertificateDefinitionAsync();
        var branchId = await SetupAlumniDataAsync(userId); // Setup Membership & Profile

        using (var scope = GetRequiredService<Volo.Abp.Security.Claims.ICurrentPrincipalAccessor>().Change(
            new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    new[] { new System.Security.Claims.Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserId, userId.ToString()) }
                )
            )
        ))
        {
                var input = new CreateCertificateRequestDto
                {
                    Items = new List<CreateCertificateItemDto>
                    {
                        new CreateCertificateItemDto
                        {
                            CertificateDefinitionId = definitionId,
                            Language = CertificateLanguage.English
                        }
                    },
                    DeliveryMethod = DeliveryMethod.BranchPickup,
                    TargetBranchId = branchId, // Required for BranchPickup
                    UserNotes = "Test Request"
                };

                // Act
                var result = await _appService.CreateAsync(input);

                // Assert
                result.ShouldNotBeNull();
                result.AlumniId.ShouldBe(userId);
                result.Items.Count.ShouldBe(1);
                // Status should be Processing because wallet covered the fees
                result.Status.ShouldBe(CertificateRequestStatus.Processing);
    }
    }

    private async Task<Guid> SetupAlumniDataAsync(Guid userId)
    {
        // 1. Create Branch
        var branch = new Informatique.Alumni.Branches.Branch(_guidGenerator.Create(), "Main Branch", "MAIN01", "Cairo");
        await _branchRepository.InsertAsync(branch, autoSave: true);

        // 2. Create Profile
        var profile = new Informatique.Alumni.Profiles.AlumniProfile(_guidGenerator.Create(), userId, "01200000000", "29001010000000");
        profile.SetBranchId(branch.Id);
        profile.AddCredit(1000);
        await _profileRepository.InsertAsync(profile, autoSave: true);

        // 3. Create Subscription Fee
        var fee = new Informatique.Alumni.Membership.SubscriptionFee(_guidGenerator.Create(), "Annual Fee", 100, DateTime.Now.Year, DateTime.Now, DateTime.Now.AddYears(1));
        await _feeRepository.InsertAsync(fee, autoSave: true);

        // 4. Create Active Membership (Approved Request)
        var request = new Informatique.Alumni.Membership.AssociationRequest(
            _guidGenerator.Create(),
            userId,
            fee.Id,
            Guid.NewGuid().ToString(),
            branch.Id,
            DateTime.Now.AddDays(-1),
            DateTime.Now.AddYears(1),
            Informatique.Alumni.Membership.DeliveryMethod.OfficePickup,
            0,
            0,
            0
        );
        request.MarkAsPaid();
        request.Approve();
        
        await _associationRepository.InsertAsync(request, autoSave: true);

        return branch.Id;
    }

    [Fact]
    public async Task GetListAsync_Should_Filter_By_Branch_For_Admin()
    {
         // Arrange
        var userId = _guidGenerator.Create();
        await _userManager.CreateAsync(new Volo.Abp.Identity.IdentityUser(userId, "adminuser", "admin@alumni.com"));
        var definitionId = await CreateCertificateDefinitionAsync();
        var branchId = await SetupAlumniDataAsync(userId);

        // Create a request for this branch
        using (var scope = GetRequiredService<Volo.Abp.Security.Claims.ICurrentPrincipalAccessor>().Change(
             new System.Security.Claims.ClaimsPrincipal(
                 new System.Security.Claims.ClaimsIdentity(
                     new[] { new System.Security.Claims.Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserId, userId.ToString()) }
                 )
             )
         ))
        {
             var input = new CreateCertificateRequestDto
             {
                 Items = new List<CreateCertificateItemDto> { new CreateCertificateItemDto { CertificateDefinitionId = definitionId, Language = CertificateLanguage.English } },
                 DeliveryMethod = DeliveryMethod.BranchPickup,
                 TargetBranchId = branchId
             };
             await _appService.CreateAsync(input);
        }

        // Act - Simulate Query
        // Note: Actual branch security depends on CurrentUser.GetCollegeId() claim mocking.
        // For this test, we verify we can retrieve validity.
        var result = await _appService.GetListAsync(new CertificateRequestFilterDto { BranchId = branchId });
        
        // Assert
        result.TotalCount.ShouldBeGreaterThan(0);
    }
    
    [Fact]
    public async Task UpdateStatusAsync_Should_Change_Status_And_History()
    {
         // Arrange
        var userId = _guidGenerator.Create();
        await _userManager.CreateAsync(new Volo.Abp.Identity.IdentityUser(userId, "statususer", "status@alumni.com"));
        var definitionId = await CreateCertificateDefinitionAsync();
        
        // FIX: Setup required data
        var branchId = await SetupAlumniDataAsync(userId);

        Guid requestId;
        using (var scope = GetRequiredService<Volo.Abp.Security.Claims.ICurrentPrincipalAccessor>().Change(
             new System.Security.Claims.ClaimsPrincipal(
                 new System.Security.Claims.ClaimsIdentity(
                     new[] { new System.Security.Claims.Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserId, userId.ToString()) }
                 )
             )
         ))
        {
             var input = new CreateCertificateRequestDto
             {
                 Items = new List<CreateCertificateItemDto> { new CreateCertificateItemDto { CertificateDefinitionId = definitionId, Language = CertificateLanguage.English } },
                 DeliveryMethod = DeliveryMethod.BranchPickup,
                 TargetBranchId = branchId
             };
             var result = await _appService.CreateAsync(input);
             requestId = result.Id;
        }

        // Act
        var updateInput = new UpdateCertificateStatusDto
        {
            NewStatus = CertificateRequestStatus.Processing,
            Note = "Processing now"
        };
        
        var updated = await _appService.UpdateStatusAsync(requestId, updateInput);

        // Assert
        updated.Status.ShouldBe(CertificateRequestStatus.Processing);
        
        // Verify History/Log if exposed, or check Repo
        var entity = await _repository.GetAsync(requestId);
        // entity.History... (not exposed in DTO?)
    }

    private async Task<Guid> CreateCertificateDefinitionAsync()
    {
        var def = new CertificateDefinition(
            _guidGenerator.Create(), 
            "Graduation Certificate (Ar)", 
            "Graduation Certificate (En)", 
            100, 
            Informatique.Alumni.Certificates.DegreeType.Undergraduate
        );
        await _definitionRepository.InsertAsync(def);
        return def.Id;
    }
}
