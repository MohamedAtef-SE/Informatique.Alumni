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
        var users = await _userRepository.GetListAsync();
        var report = new List<UserReportDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            report.Add(new UserReportDto
            {
                UserName = user.UserName,
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? "N/A",
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
