using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Profiles;

public interface IAlumniReportingAppService : IApplicationService
{
    Task<object> GetReportAsync(AlumniReportInputDto input);
    Task<List<ExpectedGraduatesReportOutputDto>> GetExpectedGraduatesReportAsync(ExpectedGraduatesReportInputDto input);
}
