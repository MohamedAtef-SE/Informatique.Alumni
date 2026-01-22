using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using Informatique.Alumni.Reporting;
using Informatique.Alumni.Directory;

namespace Informatique.Alumni.Reporting;

public class ReportingAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IReportingAppService _reportingAppService;
    private readonly Volo.Abp.Domain.Repositories.IRepository<AlumniDirectoryCache, Guid> _cacheRepository;

    public ReportingAppServiceTests()
    {
        _reportingAppService = GetRequiredService<IReportingAppService>();
        _cacheRepository = GetRequiredService<Volo.Abp.Domain.Repositories.IRepository<AlumniDirectoryCache, Guid>>();
    }

    [Fact]
    public async Task GetBasicDataReportAsync_Should_Return_CSV_Data()
    {
        // Arrange
        // Seed some directory data
        await _cacheRepository.InsertAsync(new AlumniDirectoryCache(Guid.NewGuid())
        {
            UserId = Guid.NewGuid(),
            FullName = "Report User 1",
            Email = "u1@test.com",
            JobTitle = "Job 1",
            Company = "Comp 1",
            Major = "CS",
            College = "Engineering",
            GraduationYear = 2020,
            ShowInDirectory = true
        });

        // Act
        var result = await _reportingAppService.GetBasicDataReportAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
        
        var csvContent = Encoding.UTF8.GetString(result);
        csvContent.ShouldContain("FullName,Email");
        csvContent.ShouldContain("Report User 1,u1@test.com");
    }

    [Fact]
    public async Task GetExpectedGraduatesReportAsync_Should_Filter_By_Year()
    {
        // Arrange
        var targetYear = 2025;
        await _cacheRepository.InsertAsync(new AlumniDirectoryCache(Guid.NewGuid())
        {
            UserId = Guid.NewGuid(),
            FullName = "Graduate 2025",
            Email = "grad@test.com",
            GraduationYear = targetYear,
            ShowInDirectory = true
        });

        await _cacheRepository.InsertAsync(new AlumniDirectoryCache(Guid.NewGuid())
        {
            UserId = Guid.NewGuid(),
            FullName = "Graduate 2024",
            Email = "old@test.com",
            GraduationYear = 2024,
            ShowInDirectory = true
        });

        // Act
        var result = await _reportingAppService.GetExpectedGraduatesReportAsync(targetYear);

        // Assert
        result.ShouldNotBeNull();
        var csvContent = Encoding.UTF8.GetString(result);
        csvContent.ShouldContain("Graduate 2025");
        csvContent.ShouldNotContain("Graduate 2024");
    }
}
