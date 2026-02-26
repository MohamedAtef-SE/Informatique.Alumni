using System;
using System.Threading.Tasks;
using Informatique.Alumni.Profiles;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Admin;

public interface IAlumniAdminAppService : IApplicationService
{
    Task<PagedResultDto<AlumniAdminListDto>> GetListAsync(AlumniAdminGetListInput input);
    Task<AlumniAdminDto> GetAsync(Guid id);
    Task ApproveAsync(Guid id);
    Task RejectAsync(Guid id, RejectAlumniInput input);
    Task BanAsync(Guid id);
    Task<int> SyncMemberStatusAsync();
    Task MarkAsNotableAsync(Guid id);
    Task UpdateIdCardStatusAsync(Guid id, UpdateIdCardStatusInput input);
    Task EnrichDataAsync();
}
