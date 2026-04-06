using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Informatique.Alumni.Import;

public interface IAlumniImportAppService : IApplicationService
{
    Task<AlumniImportResultDto> ImportExcelAsync(IRemoteStreamContent stream);
    Task<IRemoteStreamContent> GetImportTemplateAsync();
}
