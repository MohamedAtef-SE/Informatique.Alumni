using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Informatique.Alumni.Permissions;

namespace Informatique.Alumni.Data;

/// <summary>
/// Seeds essential data required for the application to function in production.
/// </summary>
public class DevDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityUserRepository _userRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly IPermissionManager _permissionManager;
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;
    private readonly IGuidGenerator _guidGenerator;

    private const string AdminUserName = "adminuser";
    private const string AdminUserEmail = "admin@alumni.local";
    private const string DefaultPassword = "Dev@123456";
    private const string AlumniGroupName = "Alumni";

    public DevDataSeederContributor(
        IIdentityUserRepository userRepository,
        IdentityUserManager userManager,
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager,
        IPermissionManager permissionManager,
        IPermissionDefinitionManager permissionDefinitionManager,
        IGuidGenerator guidGenerator)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _permissionManager = permissionManager;
        _permissionDefinitionManager = permissionDefinitionManager;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedAdminUserAsync();
        await CreateAlumniRoleAsync();
    }

    /// <summary>
    /// Seeds a dedicated admin user with all Admin.* permissions.
    /// Credentials: adminuser / admin@alumni.local / Dev@123456
    /// </summary>
    private async Task SeedAdminUserAsync()
    {
        var existingUser = await _userRepository.FindByNormalizedUserNameAsync(_userManager.NormalizeName(AdminUserName));
        if (existingUser != null)
        {
            // Ensure permissions are up to date
            await GrantAllAlumniPermissionsToUserAsync(existingUser.Id);
            return;
        }

        var userId = _guidGenerator.Create();
        var adminUser = new IdentityUser(userId, AdminUserName, AdminUserEmail);
        adminUser.Name = "System";
        adminUser.Surname = "Admin";
        adminUser.SetIsActive(true);
        adminUser.SetEmailConfirmed(true);

        var result = await _userManager.CreateAsync(adminUser, DefaultPassword);
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors)}");
        }

        // Assign admin role
        var adminRole = await _roleRepository.FindByNormalizedNameAsync(_roleManager.NormalizeKey("admin"));
        if (adminRole != null) await _userManager.AddToRoleAsync(adminUser, "admin");

        // Grant ALL Alumni permissions
        await GrantAllAlumniPermissionsToUserAsync(userId);
        Console.WriteLine("--- SEEDED ESSENTIAL ADMIN USER (adminuser / admin@alumni.local / Dev@123456) ---");
    }

    private async Task CreateAlumniRoleAsync()
    {
        var roleName = "alumni";
        var role = await _roleRepository.FindByNormalizedNameAsync(_roleManager.NormalizeKey(roleName));
        if (role == null)
        {
            role = new IdentityRole(_guidGenerator.Create(), roleName);
            await _roleManager.CreateAsync(role);
        }

        // Grant basic permissions to the 'alumni' role
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Events.Register, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Events.Default, true); 
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Profiles.Default, true); 
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Profiles.Edit, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Profiles.Search, true); 
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Directory.Search, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Gallery.Default, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Career.Register, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Careers.CvManage, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Careers.JobApply, true); 
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Guidance.BookSession, true); 
        
        // Membership & Benefits Access
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Membership.Default, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Membership.Request, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Benefits.Default, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Benefits.View, true);
        
        // Syndicates & Health
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Syndicates.Default, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Syndicates.Apply, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Health.Default, true);
        await _permissionManager.SetForRoleAsync(roleName, AlumniPermissions.Health.ViewOffers, true);
    }

    private async Task GrantAllAlumniPermissionsToUserAsync(Guid userId)
    {
        var allPermissions = await GetAllAlumniPermissionsAsync();
        foreach (var permission in allPermissions)
        {
            await _permissionManager.SetForUserAsync(userId, permission, true);
        }
    }

    private async Task<List<string>> GetAllAlumniPermissionsAsync()
    {
        var permissions = new List<string>();
        var groups = await _permissionDefinitionManager.GetGroupsAsync();
        var alumniGroup = groups.FirstOrDefault(g => g.Name == AlumniGroupName);
        if (alumniGroup == null) return permissions;

        foreach (var permission in alumniGroup.Permissions)
        {
            permissions.Add(permission.Name);
            AddChildPermissions(permission, permissions);
        }
        return permissions;
    }

    private void AddChildPermissions(PermissionDefinition permission, List<string> permissions)
    {
        foreach (var child in permission.Children)
        {
            permissions.Add(child.Name);
            AddChildPermissions(child, permissions);
        }
    }
}
