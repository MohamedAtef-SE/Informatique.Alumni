using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Users;
using Xunit;

namespace Informatique.Alumni.Profiles;

public class AlumniProfileAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IAlumniProfileAppService _profileAppService;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly Volo.Abp.Identity.IdentityUserManager _userManager;

    public AlumniProfileAppServiceTests()
    {
        _profileAppService = GetRequiredService<IAlumniProfileAppService>();
        _profileRepository = GetRequiredService<IRepository<AlumniProfile, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
        _currentUser = GetRequiredService<ICurrentUser>();
        _userManager = GetRequiredService<Volo.Abp.Identity.IdentityUserManager>();
    }

    [Fact]
    public async Task GetMyAsync_Should_Return_Current_User_Profile()
    {
        // Arrange
        var userId = _guidGenerator.Create();
        var user = new Volo.Abp.Identity.IdentityUser(userId, "testuser", "test@alumni.com");
        await _userManager.CreateAsync(user);

        var profileId = _guidGenerator.Create();

        var profile = new AlumniProfile(
            profileId,
            userId,
            "+1234567890",
            "12345678901234"
        );
        // profile.SetPersonalData("Test", "Alumni", DateTime.Now.AddYears(-25), Gender.Male); // Method does not exist
        profile.UpdateBasicInfo("+1234567890", "Test Bio", "Software Engineer");
        
        await _profileRepository.InsertAsync(profile);

        // Act
        AlumniProfileDto result;
        using (var scope = GetRequiredService<Volo.Abp.Security.Claims.ICurrentPrincipalAccessor>().Change(
            new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    new[] { new System.Security.Claims.Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserId, userId.ToString()) }
                )
            )
        ))
        {
            result = await _profileAppService.GetMineAsync();
        }

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(profileId);
        result.MobileNumber.ShouldBe("+1234567890");
    }
}
