using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using System.ComponentModel.DataAnnotations;

namespace Informatique.Alumni.Users;

public interface IAlumniUserAppService : IApplicationService
{
    Task CreateAsync(CreateSystemUserDto input);
    Task CreateAlumniAccountAsync(AlumniCreateDto input);
    Task CreateUserForAlumniAsync(CreateAlumniUserDto input);
    Task<List<UserReportDto>> GetSystemUsersReportAsync();
    Task<List<LoginAuditDto>> GetLoginTimingAuditAsync();
}

public class CreateAlumniUserDto
{
    [Required]
    public Guid AlumniId { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 8)]
    public string Password { get; set; } = default!;
}

public class CreateSystemUserDto
{
    [Required]
    public string UserName { get; set; } = default!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [StringLength(20, MinimumLength = 8)]
    public string Password { get; set; } = default!;

    [Required]
    public string NameAr { get; set; } = default!;

    [Required]
    public string NameEn { get; set; } = default!;

    [Required]
    public Guid BranchId { get; set; }

    public string? ProfileImage { get; set; }

    public string[] RoleNames { get; set; } = Array.Empty<string>();
}

public class AlumniCreateDto
{
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public Guid? CollegeId { get; set; }
}

public class UserReportDto
{
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public Guid? CollegeId { get; set; }
}

public class LoginAuditDto
{
    public string UserName { get; set; } = default!;
    public System.DateTime ExecutionTime { get; set; }
    public int? Duration { get; set; }
}
