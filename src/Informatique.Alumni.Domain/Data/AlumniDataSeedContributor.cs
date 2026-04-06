using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Domain.Repositories;
using System.Linq;
using System.Collections.Generic;

namespace Informatique.Alumni.Data;

public class AlumniDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly Volo.Abp.Guids.IGuidGenerator _guidGenerator;
    private readonly IRepository<Informatique.Alumni.Branches.Branch, System.Guid> _branchRepository;
    private readonly Volo.Abp.PermissionManagement.IPermissionManager _permissionManager;
    private readonly IRepository<Informatique.Alumni.Health.MedicalCategory, System.Guid> _medicalCategoryRepository;
    private readonly IRepository<Informatique.Alumni.Health.MedicalPartner, System.Guid> _medicalPartnerRepository;
    private readonly IRepository<Informatique.Alumni.Syndicates.Syndicate, System.Guid> _syndicateRepository;

    public AlumniDataSeedContributor(
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager,
        Volo.Abp.Guids.IGuidGenerator guidGenerator,
        IRepository<Informatique.Alumni.Branches.Branch, System.Guid> branchRepository,
        Volo.Abp.PermissionManagement.IPermissionManager permissionManager,
        IRepository<Informatique.Alumni.Health.MedicalCategory, System.Guid> medicalCategoryRepository,
        IRepository<Informatique.Alumni.Health.MedicalPartner, System.Guid> medicalPartnerRepository,
        IRepository<Informatique.Alumni.Syndicates.Syndicate, System.Guid> syndicateRepository)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _guidGenerator = guidGenerator;
        _branchRepository = branchRepository;
        _permissionManager = permissionManager;
        _medicalCategoryRepository = medicalCategoryRepository;
        _medicalPartnerRepository = medicalPartnerRepository;
        _syndicateRepository = syndicateRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedRolesAsync();
        await SeedBranchesAsync();
        await SeedMedicalCategoriesAsync();
        await SeedSyndicatesAsync();
    }

    private async Task SeedRolesAsync()
    {
        await CreateRoleAsync("SystemAdmin");
        await CreateRoleAsync("CollegeAdmin");
        await CreateRoleAsync("Employee");
        await CreateRoleAsync("Alumni");

        await GrantAlumniPermissionsAsync();
        await GrantEmployeePermissionsAsync();
        await GrantSystemAdminPermissionsAsync();
    }

    private async Task GrantSystemAdminPermissionsAsync()
    {
        var roleName = "SystemAdmin";
        var providerName = "R";

        // Grant all Employee permissions to SystemAdmin as well
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Admin.AlumniManage, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Admin.AlumniApprove, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Events.Manage, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Admin.ContentModerate, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Gallery.Upload, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Careers.JobManage, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Guidance.ManageRequests, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Guidance.ManageAvailability, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Certificates.Process, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Membership.Process, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Syndicates.Manage, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Health.Default, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Health.Manage, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Health.ViewOffers, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Benefits.Manage, providerName, roleName, true);
        // Companies
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Companies.Default, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Companies.Manage, providerName, roleName, true);

        // Also grant to the default ABP "admin" role
        var adminRoleName = "admin";
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Admin.AlumniManage, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Admin.AlumniApprove, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Events.Manage, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Admin.ContentModerate, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Gallery.Upload, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Careers.JobManage, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Guidance.ManageRequests, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Guidance.ManageAvailability, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Certificates.Process, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Membership.Process, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Syndicates.Manage, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Health.Default, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Health.Manage, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Health.ViewOffers, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Benefits.Manage, providerName, adminRoleName, true);
        // Companies
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Companies.Default, providerName, adminRoleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Companies.Manage, providerName, adminRoleName, true);
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

        // Health & Magazine
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Health.Default, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Health.ViewOffers, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Magazine.Default, providerName, roleName, true);

        // Trips
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Trips.Default, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Trips.Request, providerName, roleName, true);
    }

    private async Task GrantEmployeePermissionsAsync()
    {
        var roleName = "Employee";
        var providerName = "R";

        // Admin Access & Alumni Management
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Admin.AlumniManage, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Admin.AlumniApprove, providerName, roleName, true);
        
        // Content & Events
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Events.Manage, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Admin.ContentModerate, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Gallery.Upload, providerName, roleName, true);
        // Factory was invalid
        
        // Careers
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Careers.JobManage, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Careers.JobApply, providerName, roleName, true); // Maybe validation?

        // Services
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Guidance.ManageRequests, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Guidance.ManageAvailability, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Certificates.Process, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Membership.Process, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Syndicates.Manage, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Health.Default, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Health.Manage, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Health.ViewOffers, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Benefits.Manage, providerName, roleName, true);
        // Companies
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Companies.Default, providerName, roleName, true);
        await _permissionManager.SetAsync(Informatique.Alumni.Permissions.AlumniPermissions.Companies.Manage, providerName, roleName, true);
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
        System.Console.WriteLine("--- MANAGING BRANCH SEEDING ---");

        // 1. Initial Cleanup
        await DeduplicateBranchesAsync();

        // 2. Seed missing branches
        if (!await _branchRepository.AnyAsync(x => x.Name == "Makram Ebeid (HQ)"))
        {
            var cairoBranch = new Informatique.Alumni.Branches.Branch(
                _guidGenerator.Create(),
                "Makram Ebeid (HQ)",
                "HQ-01",
                "Nasr City, Cairo"
            );
            await _branchRepository.InsertAsync(cairoBranch);
            System.Console.WriteLine("--- SEEDED 'Makram Ebeid (HQ)' ---");
        }

        if (!await _branchRepository.AnyAsync(x => x.Name == "Alexandria Office"))
        {
            var alexBranch = new Informatique.Alumni.Branches.Branch(
                _guidGenerator.Create(),
                "Alexandria Office",
                "ALX-01",
                "Smouha, Alexandria"
            );
            await _branchRepository.InsertAsync(alexBranch);
             System.Console.WriteLine("--- SEEDED 'Alexandria Office' ---");
        }

        // 3. Final Cleanup (Safety Net)
        // Check again to ensure no duplicates were introduced or missed
        await DeduplicateBranchesAsync();
    }

    private async Task DeduplicateBranchesAsync()
    {
        var allBranches = await _branchRepository.GetListAsync();
        var duplicates = allBranches
            .GroupBy(b => b.Name)
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicates.Any())
        {
            System.Console.WriteLine($"--- FOUND {duplicates.Count} DUPLICATE BRANCH GROUPS. CLEANING UP... ---");
            foreach (var group in duplicates)
            {
                // Keep the one created first (assuming the first one in the list is the oldest or valid)
                var toKeep = group.OrderBy(b => b.CreationTime).First();
                var toDelete = group.Where(b => b.Id != toKeep.Id).ToList();

                await _branchRepository.DeleteManyAsync(toDelete);
                System.Console.WriteLine($"--- DELETED {toDelete.Count} DUPLICATES OF '{group.Key}' ---");
            }
        }
    }
    private async Task SeedMedicalCategoriesAsync()
    {
        if (await _medicalCategoryRepository.GetCountAsync() > 0)
        {
            return;
        }

        System.Console.WriteLine("--- SEEDING MEDICAL CATEGORIES ---");

        var categories = new List<Informatique.Alumni.Health.MedicalCategory>
        {
            new (_guidGenerator.Create(), "General Hospital", "مستشفى عام", Informatique.Alumni.Health.MedicalPartnerType.Hospital),
            new (_guidGenerator.Create(), "Pharmacy", "صيدلية", Informatique.Alumni.Health.MedicalPartnerType.Pharmacy),
            new (_guidGenerator.Create(), "Radiology Lab", "مركز أشعة", Informatique.Alumni.Health.MedicalPartnerType.Lab),
            new (_guidGenerator.Create(), "Medical Lab", "معمل تحاليل", Informatique.Alumni.Health.MedicalPartnerType.Lab),
            new (_guidGenerator.Create(), "Dental Clinic", "عيادة أسنان", Informatique.Alumni.Health.MedicalPartnerType.Clinic),
            new (_guidGenerator.Create(), "Optical Center", "مركز بصريات", Informatique.Alumni.Health.MedicalPartnerType.Clinic),
        };

        await _medicalCategoryRepository.InsertManyAsync(categories);

        // DATA MIGRATION: Migrate existing partners to these categories
        var allPartners = await _medicalPartnerRepository.GetListAsync();
        foreach (var partner in allPartners.Where(p => p.MedicalCategoryId == null))
        {
            // Try to find a matching category by text match or legacy type
            var category = categories.FirstOrDefault(c => 
                (partner.Category != null && c.NameEn.Contains(partner.Category, System.StringComparison.OrdinalIgnoreCase)) ||
                c.BaseType == partner.Type);

            if (category != null)
            {
                partner.MedicalCategoryId = category.Id;
                await _medicalPartnerRepository.UpdateAsync(partner);
            }
        }
        
        System.Console.WriteLine("--- MEDICAL CATEGORIES SEEDED & MIGRATED ---");
    }
    
    private async Task SeedSyndicatesAsync()
    {
        if (await _syndicateRepository.GetCountAsync() > 0)
        {
            return;
        }

        System.Console.WriteLine("--- SEEDING DEFAULT SYNDICATES ---");

        var syndicates = new List<Informatique.Alumni.Syndicates.Syndicate>
        {
            new (_guidGenerator.Create(), "Engineering Syndicate", "Providing professional support, insurance, and social benefits for engineers.", "ID,License,GraduationCertificate", 200),
            new (_guidGenerator.Create(), "Medical Syndicate", "Focusing on health insurance and career development for medical practitioners.", "ID,MedicalLicense,GraduationCertificate", 300),
            new (_guidGenerator.Create(), "Commerce Syndicate", "Syndicate for business and commerce professionals.", "ID,GraduationCertificate", 150),
            new (_guidGenerator.Create(), "Teachers Syndicate", "Syndicate for educational professionals and teachers.", "ID,GraduationCertificate,TeachingLicense", 120),
        };

        await _syndicateRepository.InsertManyAsync(syndicates);
        System.Console.WriteLine("--- DEFAULT SYNDICATES SEEDED ---");
    }
}
