using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Users;

public class AdminChangePasswordDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}

public class UserChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}

public interface IUserSecurityAppService : IApplicationService
{
    Task ChangePasswordForUserAsync(AdminChangePasswordDto input);
    Task ChangeMyPasswordAsync(UserChangePasswordDto input);
}
