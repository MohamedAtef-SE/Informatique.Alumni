using System;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Informatique.Alumni.Directory;

namespace Informatique.Alumni.Directory;

public class DirectoryAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IDirectoryAppService _directoryAppService;
    private readonly Volo.Abp.Domain.Repositories.IRepository<AlumniDirectoryCache, Guid> _cacheRepository;

    public DirectoryAppServiceTests()
    {
        _directoryAppService = GetRequiredService<IDirectoryAppService>();
        _cacheRepository = GetRequiredService<Volo.Abp.Domain.Repositories.IRepository<AlumniDirectoryCache, Guid>>();
    }

    [Fact]
    public async Task SearchAsync_Should_Return_Results()
    {
        // Arrange
        var cacheItem = new AlumniDirectoryCache(Guid.NewGuid())
        {
            UserId = Guid.NewGuid(),
            FullName = "John Doe",
            JobTitle = "Software Engineer",
            Company = "Tech Corp",
            Major = "Computer Science",
            College = "Engineering",
            GraduationYear = 2020,
            Email = "john.doe@example.com",
            ShowInDirectory = true
        };
        await _cacheRepository.InsertAsync(cacheItem);

        var hiddenItem = new AlumniDirectoryCache(Guid.NewGuid())
        {
            UserId = Guid.NewGuid(),
            FullName = "Jane Doe",
            JobTitle = "Hidden Engineer",
            Company = "Tech Corp",
            Major = "Computer Science",
            College = "Engineering",
            GraduationYear = 2021,
            Email = "jane.doe@example.com",
            ShowInDirectory = false
        };
        await _cacheRepository.InsertAsync(hiddenItem);

        // Act
        var result = await _directoryAppService.SearchAsync(new AlumniSearchRequestDto { Filter = "John" });

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items.ShouldContain(x => x.FullName == "John Doe");
        result.Items.ShouldNotContain(x => x.FullName == "Jane Doe");
    }

    [Fact]
    public async Task SearchAsync_Should_Filter_By_GraduationYear()
    {
        // Arrange
        await _cacheRepository.InsertAsync(new AlumniDirectoryCache(Guid.NewGuid())
        {
             UserId = Guid.NewGuid(),
             FullName = "Alumni 2020",
             GraduationYear = 2020,
             ShowInDirectory = true
        });

        await _cacheRepository.InsertAsync(new AlumniDirectoryCache(Guid.NewGuid())
        {
             UserId = Guid.NewGuid(),
             FullName = "Alumni 2021",
             GraduationYear = 2021,
             ShowInDirectory = true
        });

        // Act
        var result = await _directoryAppService.SearchAsync(new AlumniSearchRequestDto { GraduationYear = 2020 });

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items[0].FullName.ShouldBe("Alumni 2020");
    }
}
