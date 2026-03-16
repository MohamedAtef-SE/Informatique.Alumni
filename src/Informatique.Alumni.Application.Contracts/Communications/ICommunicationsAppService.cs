using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Communications;

public interface ICommunicationsAppService : IApplicationService
{
    Task<int> GetRecipientsCountAsync(AlumniCommunicationFilterDto filter);
    Task SendMessageAsync(SendGeneralMessageInputDto input);
    Task<List<int>> GetDistinctGraduationYearsAsync();
    Task<Volo.Abp.Application.Dtos.PagedResultDto<CommunicationLogDto>> SearchLogsAsync(GetCommunicationLogInputDto input);
}
