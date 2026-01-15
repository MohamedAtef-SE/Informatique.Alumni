using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Certificates;

public interface ICertificateReportingAppService : IApplicationService
{
    Task<object> GetReportAsync(CertificateReportInputDto input);
}
