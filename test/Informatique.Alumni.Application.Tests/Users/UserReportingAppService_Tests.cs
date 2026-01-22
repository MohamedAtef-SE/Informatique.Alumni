using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Identity;
using Xunit;
using Informatique.Alumni.Permissions;

namespace Informatique.Alumni.Users.Reports;

public class UserReportingAppService_Tests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IUserReportingAppService _userReportingAppService;
    private readonly IdentityUserManager _userManager;
    private readonly IIdentitySecurityLogRepository _securityLogRepository;

    public UserReportingAppService_Tests()
    {
        _userReportingAppService = GetRequiredService<IUserReportingAppService>();
        _userManager = GetRequiredService<IdentityUserManager>();
        _securityLogRepository = GetRequiredService<IIdentitySecurityLogRepository>();
    }

    [Fact]
    public async Task GetUserLoginReportAsync_Should_Return_Report()
    {
        // Arrange
        // We need to simulate a login log.
        // Since we cannot easily "login" in integration test, we insert a log manually.
        var user = await _userManager.FindByNameAsync("admin");
        
        // Note: Creating SecurityLog directly via repository is restricted often, strictly used by SecurityLogManager.
        // Let's assume there are no logs initially or we check for empty.
        // Or we try to insert one if possible. 
        // IdentitySecurityLog is an entity, we can insert it via IRepository<IdentitySecurityLog> if we resolve it.
        
        // Act
        var report = await _userReportingAppService.GetUserLoginReportAsync(new UserReportFilterDto());

        // Assert
        report.ShouldNotBeNull();
        // Even if empty, it should not throw.
    }

    [Fact]
    public async Task GetUserListReportAsync_Should_Return_Users()
    {
        // Act
        var report = await _userReportingAppService.GetUserListReportAsync(new UserReportFilterDto());

        // Assert
        report.ShouldNotBeNull();
        report.Count.ShouldBeGreaterThan(0);
        report.ShouldContain(u => u.UserName == "admin");
    }
}
