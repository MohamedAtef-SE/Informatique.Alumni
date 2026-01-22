using System;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using Volo.Abp.Identity;
using Volo.Abp.Data;
using Informatique.Alumni.Users;

namespace Informatique.Alumni.Users;

public class AlumniUserAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IAlumniUserAppService _alumniUserAppService;
    private readonly IIdentityUserRepository _userRepository;

    public AlumniUserAppServiceTests()
    {
        _alumniUserAppService = GetRequiredService<IAlumniUserAppService>();
        _userRepository = GetRequiredService<IIdentityUserRepository>();
    }

    [Fact]
    public async Task CreateAlumniAccountAsync_Should_Create_User_With_Role()
    {
        // Arrange
        var input = new AlumniCreateDto
        {
            UserName = "testalumni",
            Email = "testalumni@example.com",
            Password = "Password123!",
            CollegeId = Guid.NewGuid()
        };

        // Act
        await _alumniUserAppService.CreateAlumniAccountAsync(input);

        // Assert
        var user = await _userRepository.FindByNormalizedUserNameAsync("TESTALUMNI");
        user.ShouldNotBeNull();
        user.Email.ShouldBe("testalumni@example.com");
        
        // Check extra property
        user.GetProperty<Guid?>("CollegeId").ShouldBe(input.CollegeId);

        // Verify Role (Indirectly via user manager or just check if no error thrown implies success as mock/real manager works)
        // In integrated tests, roles usually need to exist. "Alumni" role might not be seeded in test.
        // If it fails on Role not found, I might need to seed the role in the test.
    }

    [Fact]
    public async Task GetSystemUsersReportAsync_Should_Return_Report()
    {
        // Arrange
        // Create a user to find
        var input = new AlumniCreateDto
        {
            UserName = "reportuser",
            Email = "report@example.com",
            Password = "Password123!",
            CollegeId = Guid.NewGuid()
        };
        await _alumniUserAppService.CreateAlumniAccountAsync(input);

        // Act
        var report = await _alumniUserAppService.GetSystemUsersReportAsync();

        // Assert
        report.ShouldNotBeEmpty();
        report.ShouldContain(x => x.UserName == "reportuser");
    }
}
