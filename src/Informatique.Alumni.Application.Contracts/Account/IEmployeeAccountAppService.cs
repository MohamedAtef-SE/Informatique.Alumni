using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Account;

public class ResetPasswordInput
{
    [Required]
    public string UserName { get; set; } = string.Empty;
}

public interface IEmployeeAccountAppService : IApplicationService
{
    Task ResetPasswordByEmailAsync(ResetPasswordInput input);
}
