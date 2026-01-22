using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Profiles;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using Volo.Abp.Guids;
using Volo.Abp.Users;
using Xunit;

namespace Informatique.Alumni.Membership;

public class MembershipAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IMembershipAppService _membershipAppService;
    private readonly IRepository<AssociationRequest, Guid> _requestRepository;
    private readonly IRepository<SubscriptionFee, Guid> _feeRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IGuidGenerator _guidGenerator;

    public MembershipAppServiceTests()
    {
        _membershipAppService = GetRequiredService<IMembershipAppService>();
        _requestRepository = GetRequiredService<IRepository<AssociationRequest, Guid>>();
        _feeRepository = GetRequiredService<IRepository<SubscriptionFee, Guid>>();
        _profileRepository = GetRequiredService<IRepository<AlumniProfile, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task RequestMembershipAsync_Should_Create_Request_And_Deduct_Wallet()
    {
        // Arrange
        var userId = _guidGenerator.Create();
        var currentUser = GetRequiredService<Volo.Abp.Users.ICurrentUser>();
        
        // Setup Fee
        var fee = new SubscriptionFee(
            _guidGenerator.Create(),
            "2024 Membership",
            100m,
            2024,
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(20)
        );
        await _feeRepository.InsertAsync(fee);

        // Setup User
        var userManager = GetRequiredService<Volo.Abp.Identity.IdentityUserManager>();
        var user = new Volo.Abp.Identity.IdentityUser(userId, "testuser", "test@alumni.com");
        await userManager.CreateAsync(user);

        // Setup Profile with Wallet
        var profileId = _guidGenerator.Create();
        var profile = new AlumniProfile(
            profileId,
            userId, // Link to the Current User
            "+1234567890",
            "12345678901234"
        );
        profile.AddCredit(150m); // Enough to pay 100
        
        // Add Education
        var education = new Education(
            _guidGenerator.Create(),
            profile.Id,
            "University", 
            "Bachelor", 
            2023
        );
        education.SetAcademicDetails(1, _guidGenerator.Create(), _guidGenerator.Create(), null);
        profile.AddEducation(education);
        
        await _profileRepository.InsertAsync(profile);

        var input = new CreateAssociationRequestDto
        {
            SubscriptionFeeId = fee.Id,
            TargetBranchId = _guidGenerator.Create(),
            DeliveryMethod = DeliveryMethod.OfficePickup,
            IdempotencyKey = Guid.NewGuid().ToString(),
            Emails = new List<ContactUpdateDto>(),
            Mobiles = new List<ContactUpdateDto>()
        };

        var currentPrincipalAccessor = GetRequiredService<Volo.Abp.Security.Claims.ICurrentPrincipalAccessor>();
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserId, userId.ToString())
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);

        // Act
        AssociationRequestDto result;
        using (currentPrincipalAccessor.Change(principal))
        {
            result = await _membershipAppService.RequestMembershipAsync(input);
        }

        // Assert
        result.ShouldNotBeNull();
        result.UsedWalletAmount.ShouldBe(100m); // Total fee covered (100)
        result.RemainingAmount.ShouldBe(0m);
        result.Status.ShouldBe(MembershipRequestStatus.Paid); // Should be paid immediately

        // Verify Profile Wallet Update
        var updatedProfile = await _profileRepository.GetAsync(profileId);
        updatedProfile.WalletBalance.ShouldBe(50m); // 150 - 100
    }
}
