using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Application.Dtos;
using Informatique.Alumni.Branches;
using System.Collections.Generic;

namespace Informatique.Alumni.Branches;

public class BranchAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IBranchAppService _branchAppService;

    public BranchAppServiceTests()
    {
        _branchAppService = GetRequiredService<IBranchAppService>();
    }

    [Fact]
    public async Task GetAcademicStructureAsync_Should_Return_List()
    {
        // Act
        // Simulate Authenticated User to bypass [Authorize]
        using (var scope = GetRequiredService<Volo.Abp.Security.Claims.ICurrentPrincipalAccessor>().Change(
            new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    new[] { new System.Security.Claims.Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserId, Guid.NewGuid().ToString()) }
                )
            )
        ))
        {
            var result = await _branchAppService.GetAcademicStructureAsync();

            // Assert
            result.ShouldNotBeNull();
        }
    }
}
