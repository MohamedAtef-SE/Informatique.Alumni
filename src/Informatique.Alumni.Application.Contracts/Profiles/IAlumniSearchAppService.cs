using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Application service for querying and managing alumni profiles by employees.
/// </summary>
public interface IAlumniSearchAppService : IApplicationService
{
    /// <summary>
    /// Search alumni with filtering, pagination, and branch scoping.
    /// Returns latest qualification for each alumni in the list.
    /// </summary>
    Task<PagedResultDto<AlumniListDto>> GetListAsync(AlumniSearchFilterDto input);
    
    /// <summary>
    /// Get detailed profile of an alumni.
    /// Shows all qualifications (history).
    /// </summary>
    Task<AlumniProfileDetailDto> GetProfileAsync(Guid id);
    
    /// <summary>
    /// Update alumni personal photo (Employee Exception).
    /// </summary>
    Task UpdatePhotoAsync(Guid id, UpdateAlumniPhotoDto input);
}
