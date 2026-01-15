using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Directory;

public interface IDirectoryAppService : IApplicationService
{
    Task<PagedResultDto<AlumniDirectoryDto>> SearchAsync(AlumniSearchRequestDto input);
}
