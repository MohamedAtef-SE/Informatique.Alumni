using Shouldly;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Application.Dtos;
using Informatique.Alumni.Health;

namespace Informatique.Alumni.Health;

public class MedicalPartnerAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IMedicalPartnerAppService _medicalPartnerAppService;

    public MedicalPartnerAppServiceTests()
    {
        _medicalPartnerAppService = GetRequiredService<IMedicalPartnerAppService>();
    }

    [Fact]
    public async Task GetListAsync_Should_Return_List()
    {
        // Act
        var result = await _medicalPartnerAppService.GetListAsync(null);

        // Assert
        result.ShouldNotBeNull();
    }
}
