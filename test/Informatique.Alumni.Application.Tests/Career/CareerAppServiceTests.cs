using Shouldly;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Career;

public class CareerAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly ICareerAppService _careerAppService;

    public CareerAppServiceTests()
    {
        _careerAppService = GetRequiredService<ICareerAppService>();
    }

    [Fact]
    public async Task GetServicesAsync_Should_Return_List()
    {
        // Act
        var result = await _careerAppService.GetServicesAsync(new PagedAndSortedResultRequestDto());

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
    }
}
