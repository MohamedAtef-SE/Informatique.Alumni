using Shouldly;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Application.Dtos;
using Informatique.Alumni.Gallery;

namespace Informatique.Alumni.Gallery;

public class GalleryAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IGalleryAppService _galleryAppService;

    public GalleryAppServiceTests()
    {
        _galleryAppService = GetRequiredService<IGalleryAppService>();
    }

    [Fact]
    public async Task GetListAsync_Should_Return_List()
    {
        // Act
        var result = await _galleryAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
    }
}
