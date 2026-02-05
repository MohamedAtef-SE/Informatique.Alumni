using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Data;

public class AlumniDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly Volo.Abp.Guids.IGuidGenerator _guidGenerator;
    private readonly IRepository<Informatique.Alumni.Branches.Branch, System.Guid> _branchRepository;
    private readonly Volo.Abp.PermissionManagement.IPermissionManager _permissionManager;

    public AlumniDataSeedContributor(
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager,
        Volo.Abp.Guids.IGuidGenerator guidGenerator,
        IRepository<Informatique.Alumni.Branches.Branch, System.Guid> branchRepository,
        Volo.Abp.PermissionManagement.IPermissionManager permissionManager)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _guidGenerator = guidGenerator;
        _branchRepository = branchRepository;
        _permissionManager = permissionManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedRolesAsync();
        await SeedBranchesAsync();
    }

    private async Task SeedRolesAsync()
    {
        await CreateRoleAsync("SystemAdmin");
        await CreateRoleAsync("CollegeAdmin");
        await CreateRoleAsync("Employee");
        await CreateRoleAsync("Alumni");

        await GrantAlumniPermissionsAsync();
    }

    private async Task GrantAlumniPermissionsAsync()
    {
        var roleName = "Alumni";
        var providerName = "R"; // RolePermissionValueProvider.ProviderName;

        // Profiles
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Profiles.Default, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Profiles.Edit, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Profiles.ViewAll, providerName, roleName, true);
        
        // Events
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Events.Default, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Events.Register, providerName, roleName, true);
        
        // Benefits
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Benefits.Default, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Benefits.View, providerName, roleName, true);

        // Directory
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Directory.Default, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Directory.Search, providerName, roleName, true);

        // Career
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Career.Default, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Career.Register, providerName, roleName, true);
        
        // Certificates
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Certificates.Default, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Certificates.Request, providerName, roleName, true);

        // Trips
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Trips.Default, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Trips.Request, providerName, roleName, true);
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

    private async Task SeedBranchesAsync()
    {
        System.Console.WriteLine("--- FORCING BRANCH SEEDING ---");
        // if (await _branchRepository.GetCountAsync() > 0)
        // {
        //     return;
        // }

        var cairoBranch = new Informatique.Alumni.Branches.Branch(
            _guidGenerator.Create(),
            "Makram Ebeid (HQ)",
            "HQ-01",
            "Nasr City, Cairo"
        );

        var alexBranch = new Informatique.Alumni.Branches.Branch(
            _guidGenerator.Create(),
            "Alexandria Office",
            "ALX-01",
            "Smouha, Alexandria"
        );

        await _branchRepository.InsertAsync(cairoBranch);
        await _branchRepository.InsertAsync(alexBranch);
    }
}
