using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;

namespace Informatique.Alumni.Career;

public class JobAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IJobAppService _jobAppService;
    private readonly IRepository<Job, Guid> _jobRepository;
    private readonly IGuidGenerator _guidGenerator;

    public JobAppServiceTests()
    {
        _jobAppService = GetRequiredService<IJobAppService>();
        _jobRepository = GetRequiredService<IRepository<Job, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task GetJobsAsync_Should_Return_Active_Jobs()
    {
        // Arrange
        var companyId = _guidGenerator.Create();
        var job = new Job(
            _guidGenerator.Create(),
            companyId,
            "Software Engineer",
            "Senior Developer needed"
        );
        job.SetClosingDate(DateTime.Now.AddDays(10));
        
        await _jobRepository.InsertAsync(job);

        // Act
        // Authorization bypass might be needed if GetJobsAsync is protected, 
        // but typically GetList is AllowAnonymous or weak auth. 
        // Checking JobAppService: [Authorize] but GetJobsAsync is public? 
        // The class has [Authorize]. Let's mock a user.
        
        var userId = _guidGenerator.Create();
        using (var scope = GetRequiredService<Volo.Abp.Security.Claims.ICurrentPrincipalAccessor>().Change(
            new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    new[] { new System.Security.Claims.Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserId, userId.ToString()) }
                )
            )
        ))
        {
            var result = await _jobAppService.GetJobsAsync(new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto());

            // Assert
            result.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
            result.Items.ShouldContain(j => j.Title == "Software Engineer");
        }
    }
}
