using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Users;

public interface IAlumniUserAppService : IApplicationService
{
    Task CreateAlumniAccountAsync(AlumniCreateDto input);
    Task<List<UserReportDto>> GetSystemUsersReportAsync();
    Task<List<LoginAuditDto>> GetLoginTimingAuditAsync();
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
