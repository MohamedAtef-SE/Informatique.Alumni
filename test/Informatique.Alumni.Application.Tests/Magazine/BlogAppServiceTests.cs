using Shouldly;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Application.Dtos;
using Informatique.Alumni.Magazine;
using System;

namespace Informatique.Alumni.Magazine;

public class BlogAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IBlogAppService _blogAppService;

    public BlogAppServiceTests()
    {
        _blogAppService = GetRequiredService<IBlogAppService>();
    }

    [Fact]
    public async Task GetPostsAsync_Should_Return_List()
    {
        // Act
        var result = await _blogAppService.GetListAsync(new PostSearchInputDto());

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
    }
}
