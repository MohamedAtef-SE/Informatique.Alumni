using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Account;

public class GraduateLoginDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public interface IGraduateAuthAppService : IApplicationService
{
    Task LoginAsync(GraduateLoginDto input);
    Task ForgotPasswordAsync(string username);
}
