using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace Informatique.Alumni.Data;

public class AlumniDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly Volo.Abp.Guids.IGuidGenerator _guidGenerator;

    public AlumniDataSeedContributor(
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager,
        Volo.Abp.Guids.IGuidGenerator guidGenerator)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedRolesAsync();
    }

    private async Task SeedRolesAsync()
    {
        await CreateRoleAsync("SystemAdmin");
        await CreateRoleAsync("CollegeAdmin");
        await CreateRoleAsync("Employee");
        await CreateRoleAsync("Alumni");
    }

    private async Task CreateRoleAsync(string roleName)
    {
        var role = await _roleRepository.FindByNormalizedNameAsync(_roleManager.NormalizeKey(roleName));
        if (role == null)
        {
            role = new IdentityRole(_guidGenerator.Create(), roleName);
            await _roleManager.CreateAsync(role);
        }
    }
}
