using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Data;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Profiles;

namespace Informatique.Alumni.Users.Reports;

[Authorize]
public class UserReportingAppService : ApplicationService, IUserReportingAppService
{
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<IdentitySecurityLog, Guid> _securityLogRepository;
    private readonly IRepository<AlumniProfile, Guid> _alumniProfileRepository;

    public UserReportingAppService(
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<IdentitySecurityLog, Guid> securityLogRepository,
        IRepository<AlumniProfile, Guid> alumniProfileRepository)
    {
        _userRepository = userRepository;
        _securityLogRepository = securityLogRepository;
        _alumniProfileRepository = alumniProfileRepository;
    }

    [Authorize(AlumniPermissions.Users.LoginAuditReport)]
    public async Task<List<UserLoginReportDto>> GetUserLoginReportAsync(UserReportFilterDto input)
    {
        var logQueryable = await _securityLogRepository.GetQueryableAsync();
        
        // Filter for Login actions
        var query = logQueryable.Where(l => l.Action == "Login");

        // Execute GroupBy in memory due to EF Core limitations with some providers or just simpler logic
        // But for reporting, we should try database side if possible.
        // EF Core 8 supports GroupBy better.
        
        // However, standard GroupBy with "Max object" is tricky.
        // Let's get the list of recent logins.
        
        // Alternative: Select specific columns first
        var logs = query.Select(l => new { l.UserId, l.CreationTime, l.UserName });
        
        // ToListAsync() then GroupBy in memory is safer for complex projections
        // LIMITATION: Large dataset. 
        // OPTIMIZATION: Date filter? The requirement doesn't specify date range, but usually needed.
        // We will assume reasonable volume or just take the hit for now as per requirements.
        
        var flatLogs = await AsyncExecuter.ToListAsync(logs);
        
        var latestLogStats = flatLogs
            .GroupBy(l => l.UserId)
            .Select(g => new 
            { 
                UserId = g.Key, 
                LastLoginUtc = g.Max(l => l.CreationTime),
                UserName = g.First().UserName // Taken from group
            })
            .ToList();

        var report = new List<UserLoginReportDto>();

        foreach (var stat in latestLogStats)
        {
            if (stat.UserId == Guid.Empty) continue;
            
            report.Add(new UserLoginReportDto
            {
                UserName = stat.UserName ?? "Unknown",
                LastLoginDate = stat.LastLoginUtc,
                LastLoginTime = stat.LastLoginUtc.ToString("HH:mm:ss"),
                UserType = "System User" // Placeholder
            });
        }
        
        if (!string.IsNullOrEmpty(input.UserType))
        {
           // Implementation for UserType filtering would go here
        }

        return report.OrderByDescending(r => r.LastLoginDate).ToList();
    }

    [Authorize(AlumniPermissions.Users.SystemUsersReport)]
    public async Task<List<UserStatusReportDto>> GetUserListReportAsync(UserReportFilterDto input)
    {
        var queryable = await _userRepository.GetQueryableAsync();

        if (input.IsActive.HasValue)
        {
            queryable = queryable.Where(u => u.IsActive == input.IsActive.Value);
        }

        // Security Scoped Branch Filter
        if (!CurrentUser.IsInRole("admin"))
        {
             var userBranchIdClaim = CurrentUser.FindClaim("BranchId");
             if (userBranchIdClaim != null && Guid.TryParse(userBranchIdClaim.Value, out var branchId))
             {
                 input.BranchId = branchId; 
             }
        }
        
        var users = await AsyncExecuter.ToListAsync(queryable);
        
        // Filter by Branch Memory-side (ExtraProperty)
        if (input.BranchId.HasValue)
        {
             users = users.Where(u => u.GetProperty<Guid?>("BranchId") == input.BranchId.Value).ToList();
        }

        var userIds = users.Select(u => u.Id).ToList();
        var profiles = await _alumniProfileRepository.GetListAsync(p => userIds.Contains(p.UserId));
        var profileDict = profiles.ToDictionary(p => p.UserId);

        var report = new List<UserStatusReportDto>();

        foreach (var user in users)
        {
            string jobTitle = "N/A";
            if (profileDict.TryGetValue(user.Id, out var profile))
            {
                jobTitle = profile.JobTitle;
            }
            
            report.Add(new UserStatusReportDto
            {
                UserName = user.UserName,
                FullName = $"{user.Name} {user.Surname}".Trim(),
                Status = user.IsActive ? "Active" : "Inactive",
                JobTitle = jobTitle,
                BranchName = "Main Branch" 
            });
        }

        return report.OrderBy(r => r.FullName).ToList();
    }
}
