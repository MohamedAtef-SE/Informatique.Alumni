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
using Informatique.Alumni.Permissions;

namespace Informatique.Alumni.Users;

[Authorize]
public class AlumniUserAppService : ApplicationService, IAlumniUserAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public AlumniUserAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        IAuditLogRepository auditLogRepository)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
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
