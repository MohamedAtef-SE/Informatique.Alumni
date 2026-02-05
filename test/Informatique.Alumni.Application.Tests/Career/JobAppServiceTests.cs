using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;
using Volo.Abp;
using Volo.Abp.Uow;

namespace Informatique.Alumni.Career;

public class JobAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IJobAppService _jobAppService;
    private readonly IRepository<Job, Guid> _jobRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly Volo.Abp.Uow.IUnitOfWorkManager _unitOfWorkManager;

    public JobAppServiceTests()
    {
        _jobAppService = GetRequiredService<IJobAppService>();
        _jobRepository = GetRequiredService<IRepository<Job, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
        _unitOfWorkManager = GetRequiredService<Volo.Abp.Uow.IUnitOfWorkManager>();
    }

    [Fact]
    public async Task ApplyAsync_Should_Fail_If_No_CV()
    {
        // Arrange
        var companyId = _guidGenerator.Create();
        var job = new Job(_guidGenerator.Create(), companyId, "Test Job No CV", "Desc");
        await _jobRepository.InsertAsync(job);

        // Act & Assert
        var userId = _guidGenerator.Create();
        using (var scope = GetRequiredService<Volo.Abp.Security.Claims.ICurrentPrincipalAccessor>().Change(
            new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    new[] { new System.Security.Claims.Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserId, userId.ToString()) }
                )
            )
        ))
        {
            var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () => 
            {
                await _jobAppService.ApplyAsync(job.Id);
            });
            exception.Message.ShouldContain("create a CV");
            exception.Code.ShouldBe("Career:NoCvFound");
        }
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

    [Fact]
    public async Task ApplyAsync_Should_Create_Application_Successfully()
    {
        // Arrange - Create a job
        var companyId = _guidGenerator.Create();
        var job = new Job(_guidGenerator.Create(), companyId, "Test Job", "Test Description");
        await _jobRepository.InsertAsync(job);

        // Act - Apply as a user with correct permission
        var userId = _guidGenerator.Create();
        
        // Ensure User has CV
        using (var uow = _unitOfWorkManager.Begin())
        {
            var cvRepo = GetRequiredService<IRepository<CurriculumVitae, Guid>>();
            await cvRepo.InsertAsync(new CurriculumVitae(_guidGenerator.Create(), userId));
            await uow.CompleteAsync();
        }

        using (var scope = GetRequiredService<Volo.Abp.Security.Claims.ICurrentPrincipalAccessor>().Change(
            new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    new[] { 
                        new System.Security.Claims.Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserId, userId.ToString()),
                        // Note: In real tests, permission should be granted via role assignment
                    },
                    "TestAuth"
                )
            )
        ))
        {
            // This test verifies the core logic works - actual auth is tested separately
            await _jobAppService.ApplyAsync(job.Id);
        }

        // Assert - Verify application was created
        using (var uow = _unitOfWorkManager.Begin())
        {
            var applicationRepo = GetRequiredService<IRepository<JobApplication, Guid>>();
            var exists = await applicationRepo.AnyAsync(a => a.JobId == job.Id && a.AlumniId == userId);
            exists.ShouldBeTrue();
        }
    }
}
