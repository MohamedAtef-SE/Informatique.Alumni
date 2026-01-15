using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Reporting;

public interface IReportingAppService : IApplicationService
{
    Task<byte[]> GetBasicDataReportAsync();
    Task<byte[]> GetExpectedGraduatesReportAsync(int year);
}
