using System;
using System.Threading.Tasks;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Admin;

public interface IAlumniAdminAppService : IApplicationService
{
    Task<PagedResultDto<AlumniAdminListDto>> GetListAsync(AlumniAdminGetListInput input);
    [HttpGet]
    [Route("{id}")]
    Task<AlumniAdminDto> GetAsync(Guid id);

    [HttpPost]
    [Route("{id}/approve")]
    Task ApproveAsync(Guid id);

    [HttpPost]
    [Route("{id}/reject")]
    Task RejectAsync(Guid id, RejectAlumniInput input);

    [HttpPost]
    [Route("{id}/ban")]
    Task BanAsync(Guid id);

    [HttpPost]
    [Route("sync-status")]
    Task<int> SyncMemberStatusAsync();

    [HttpPost]
    [Route("{id}/mark-as-notable")]
    Task MarkAsNotableAsync(Guid id);

    [HttpPost]
    [Route("{id}/toggle-advisor")]
    Task ToggleAdvisorAsync(Guid id);

    [HttpPost]
    [Route("{id}/approve-advisor")]
    Task ApproveAdvisorAsync(Guid id);

    [HttpPost]
    [Route("{id}/reject-advisor")]
    Task RejectAdvisorAsync(Guid id, RejectAlumniInput input);

    [HttpPost]
    [Route("{id}/id-card-status")]
    Task UpdateIdCardStatusAsync(Guid id, UpdateIdCardStatusInput input);

    [HttpPost]
    [Route("enrich-data")]
    Task EnrichDataAsync();

    [HttpGet]
    [Route("profiles")]
    Task<PagedResultDto<AlumniAdminListDto>> GetProfilesAsync(AlumniAdminGetListInput input);
}
