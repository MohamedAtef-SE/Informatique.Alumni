using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Common;

public interface IFileAppService : IApplicationService
{
    Task<string> UploadAsync(IFormFile file);
}
