using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Informatique.Alumni.Career;

public class CvAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly ICVAppService _cvAppService;
    private readonly IRepository<CurriculumVitae, Guid> _cvRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IPermissionManager _permissionManager;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public CvAppServiceTests()
    {
        _cvAppService = GetRequiredService<ICVAppService>();
        _cvRepository = GetRequiredService<IRepository<CurriculumVitae, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
        _permissionManager = GetRequiredService<IPermissionManager>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    [Fact]
    public async Task SanityCheck_Repository_Should_Work()
    {
        var count = await _cvRepository.GetCountAsync();
        count.ShouldBeGreaterThanOrEqualTo(0);

        var cv = new CurriculumVitae(_guidGenerator.Create(), _guidGenerator.Create());
        await _cvRepository.InsertAsync(cv);
    }
/*
    [Fact]
    public async Task GetMyCvAsync_Should_Create_CV_If_Not_Exists()
    {
        // ...
    }
*/
}
