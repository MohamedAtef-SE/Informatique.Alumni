using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Users.Reports;

public class UserLoginReportDto
{
    public string UserName { get; set; } = string.Empty;
    public DateTime? LastLoginDate { get; set; }
    public string LastLoginTime { get; set; } = string.Empty; // Format as HH:mm:ss
    public string UserType { get; set; } = string.Empty; // Admin or Graduate
}

public class UserStatusReportDto
{
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Active / Inactive
    public string JobTitle { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
}

public class UserReportFilterDto 
{
    public Guid? BranchId { get; set; }
    public bool? IsActive { get; set; }
    public string? UserType { get; set; } // "Admin" or "Graduate"
}

public interface IUserReportingAppService : IApplicationService
{
    Task<List<UserLoginReportDto>> GetUserLoginReportAsync(UserReportFilterDto input);
    Task<List<UserStatusReportDto>> GetUserListReportAsync(UserReportFilterDto input);
}
