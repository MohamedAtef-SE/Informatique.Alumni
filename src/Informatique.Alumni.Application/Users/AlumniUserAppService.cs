using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp.AuditLogging;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Informatique.Alumni.Permissions;

namespace Informatique.Alumni.Users;

[Authorize]
public class AlumniUserAppService : ApplicationService, IAlumniUserAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IRepository<Informatique.Alumni.Profiles.AlumniProfile, Guid> _alumniProfileRepository;
    private readonly Volo.Abp.Emailing.IEmailSender _emailSender;
    private readonly Informatique.Alumni.Infrastructure.SMS.ISmsSender _smsSender;
    private readonly IConfiguration _configuration;

    public AlumniUserAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        IRepository<Informatique.Alumni.Profiles.AlumniProfile, Guid> alumniProfileRepository,
        Volo.Abp.Emailing.IEmailSender emailSender,
        Informatique.Alumni.Infrastructure.SMS.ISmsSender smsSender,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _alumniProfileRepository = alumniProfileRepository;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _configuration = configuration;
    }

    [Authorize("AbpIdentity.Users.Create")]
    public async Task CreateUserForAlumniAsync(CreateAlumniUserDto input)
    {
        // 1. Fetch Alumni Profile
        var alumni = await _alumniProfileRepository.GetAsync(input.AlumniId);
        
        // 2. Check One-to-One Constraint (if User already linked)
        // Note: Using GetListAsync with filter for mapped property or custom query would be better, 
        // but for now, we assume simple check. EF Core query might be needed for perf.
        // Assuming UserId field on AlumniProfile is updated when user is created.
        if (alumni.UserId != Guid.Empty)
        {
             // Verify if that user actually exists to avoid broken link issues
             var existingUser = await _userManager.FindByIdAsync(alumni.UserId.ToString());
             if (existingUser != null)
             {
                 throw new Volo.Abp.UserFriendlyException($"Alumni with ID {input.AlumniId} already has a system user account ({existingUser.UserName}).");
             }
        }

        // 3. Credentials Generation (Username = NationalId or some ID? User Story says "Alumni Number")
        // Alumni entity doesn't show "AlumniNumber" in the viewed file, it shows NationalId.
        // Assuming NationalId is the unique identifier to use as UserName per context, or we should use ID.
        // Let's use NationalId as "Alumni Number" proxy if AlumniNumber field is missing, or strictly follow "Alumni Number".
        // The viewed file `AlumniProfile.cs` does NOT have `AlumniNumber`. 
        // However, standard education systems use Student ID. 
        // I will use `NationalId` as the Username for now as it is unique and mandatory in the entity I saw.
        // CHECK: If the requirement strictly says "Read-Only/Fixed Alumni ID", I should use that.
        // `NationalId` is `Check.NotNullOrWhiteSpace`.
        string userName = alumni.NationalId; 

        // Check if user exists by name (Double check)
        var userCheck = await _userManager.FindByNameAsync(userName);
        if (userCheck != null)
        {
            throw new Volo.Abp.UserFriendlyException($"User with username {userName} already exists.");
        }

        // 4. Create IdentityUser
        var user = new IdentityUser(GuidGenerator.Create(), userName, alumni.Emails.FirstOrDefault(e => e.IsPrimary)?.Email ?? $"{userName}@alumni.com", CurrentTenant.Id);
        
        // Extended Properties
        user.SetProperty("NameAr", "Alumni User"); // Should fetch from Profile if available, but Profile only has Bio/Job. Assuming generic or separate entity for Name. IdentityUser has Name/Surname.
        user.SetProperty("NameEn", "Alumni User");
        user.SetProperty("BranchId", alumni.BranchId);
        user.SetProperty("CollegeId", Guid.Empty); // Unknown context, set default.

        (await _userManager.CreateAsync(user, input.Password)).CheckErrors();
        
        // 5. Assign Role
        (await _userManager.AddToRoleAsync(user, "Graduate")).CheckErrors();

        // 6. Link Alumni to User
        alumni.SetUserId(user.Id);
        await _alumniProfileRepository.UpdateAsync(alumni);
        
        // 7. Notification
        // Email
        await _emailSender.SendAsync(
            user.Email,
            "Your Alumni System Access",
            $"Welcome! Your username is {userName} and password is {input.Password}. Login here: {_configuration["App:SelfUrl"]}"
        );

        // SMS (If Mobile exists)
        if (!string.IsNullOrEmpty(alumni.MobileNumber))
        {
            await _smsSender.SendAsync(
                alumni.MobileNumber,
                $"Alumni System Access: User={userName}, Pass={input.Password}"
            );
        }
    }

    [Authorize("AbpIdentity.Users.Create")]
    public async Task CreateAsync(CreateSystemUserDto input)
    {
        // Branch Scoping Logic
        // If current user is not Super Admin (admin role), restrict to their own branch.
        if (!CurrentUser.IsInRole("admin"))
        {
            var userBranchIdClaim = CurrentUser.FindClaim("BranchId");
            if (userBranchIdClaim != null && Guid.TryParse(userBranchIdClaim.Value, out var branchId))
            {
                if (input.BranchId != branchId)
                {
                    throw new Volo.Abp.UserFriendlyException("Security Error: You can only create users for your assigned branch.");
                }
            }
        }

        var user = new IdentityUser(GuidGenerator.Create(), input.UserName, input.Email, CurrentTenant.Id);
        
        // Extension Properties
        user.SetProperty("NameAr", input.NameAr);
        user.SetProperty("NameEn", input.NameEn);
        user.SetProperty("BranchId", input.BranchId);
        if (!string.IsNullOrEmpty(input.ProfileImage))
        {
            user.SetProperty("ProfileImage", input.ProfileImage);
        }
        
        // Create User
        (await _userManager.CreateAsync(user, input.Password)).CheckErrors();
        
        // Assign Roles
        if (input.RoleNames != null && input.RoleNames.Any())
        {
            (await _userManager.AddToRolesAsync(user, input.RoleNames)).CheckErrors();
        }
    }

    [Authorize(AlumniPermissions.Users.CreateAlumni)]
    public async Task CreateAlumniAccountAsync(AlumniCreateDto input)
    {
        var user = new IdentityUser(GuidGenerator.Create(), input.UserName, input.Email);
        user.SetProperty("CollegeId", input.CollegeId);
        
        (await _userManager.CreateAsync(user, input.Password)).CheckErrors();
        (await _userManager.AddToRoleAsync(user, "Alumni")).CheckErrors();
    }

    [Authorize(AlumniPermissions.Users.SystemUsersReport)]
    public async Task<List<UserReportDto>> GetSystemUsersReportAsync()
    {
        // Efficiently fetch users with details (includes roles)
        var users = await _userRepository.GetListAsync(includeDetails: true);
        
        // Fetch all roles to map RoleId to RoleName
        // Note: IIdentityRoleRepository is needed ideally, but we can assume basic mapping or fetch all if possible.
        // Or cleaner: Use IdentityUserManager to optimize? No, manager is usually single-user focused.
        // Let's use LazyServiceProvider or similar if role repo is missing, but typically we can get roles.
        // For now, simpler optimization: 
        // Iterate users, but since we have "includeDetails: true", `user.Roles` is populated.
        // `user.Roles` contains `IdentityUserRole` objects (UserId, RoleId).
        // We still need Role Names.
        // Bulk fetch roles.
        var roleRepo = LazyServiceProvider.LazyGetRequiredService<IIdentityRoleRepository>();
        var allRoles = await roleRepo.GetListAsync();
        var roleDict = allRoles.ToDictionary(r => r.Id, r => r.Name);

        var report = new List<UserReportDto>();

        foreach (var user in users)
        {
            // Roles logic
            var roleNames = user.Roles
                .Select(ur => roleDict.ContainsKey(ur.RoleId) ? roleDict[ur.RoleId] : null)
                .Where(n => n != null)
                .ToList();

            report.Add(new UserReportDto
            {
                UserName = user.UserName,
                Email = user.Email,
                Role = roleNames.FirstOrDefault() ?? "N/A", // Taking first role as per previous logic
                CollegeId = user.GetProperty<Guid?>("CollegeId")
            });
        }

        return report;
    }

    [Authorize(AlumniPermissions.Users.LoginAuditReport)]
    public async Task<List<LoginAuditDto>> GetLoginTimingAuditAsync()
    {
        // Filter by login actions. Note: Audit logging might need specific configuration to capture this clearly.
        // For simplicity, we search for 'Login' in the URL or similar if using built-in ABP login.
        var logs = await _auditLogRepository.GetListAsync(
            url: "/Account/Login", 
            httpMethod: "POST",
            maxResultCount: 100);

        return logs.Select(l => new LoginAuditDto
        {
            UserName = l.UserName,
            ExecutionTime = l.ExecutionTime,
            Duration = l.ExecutionDuration
        }).ToList();
    }
}
