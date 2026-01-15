using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Career;

public interface ICareerReportingAppService : IApplicationService
{
    Task<List<object>> GetParticipantsReportAsync(CareerParticipantReportInputDto input);
    Task<List<object>> GetServicesReportAsync(CareerServicesReportInputDto input);
}
