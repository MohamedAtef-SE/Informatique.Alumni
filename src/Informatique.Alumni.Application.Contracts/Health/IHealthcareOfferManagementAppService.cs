using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Health;

/// <summary>
/// Application service interface for healthcare offer management (employee operations).
/// Requires: Healthcare.Manage permission.
/// Full CRUD operations with file upload/replacement support.
/// </summary>
public interface IHealthcareOfferManagementAppService : IApplicationService
{
    /// <summary>
    /// Gets a paged list of healthcare offers with optional filtering.
    /// </summary>
    Task<PagedResultDto<HealthcareOfferDto>> GetListAsync(GetHealthcareOffersInput input);
    
    /// <summary>
    /// Gets a single healthcare offer by ID.
    /// </summary>
    Task<HealthcareOfferDto> GetAsync(Guid id);
    
    /// <summary>
    /// Creates a new healthcare offer with file upload.
    /// </summary>
    Task<HealthcareOfferDto> CreateAsync(CreateHealthcareOfferDto input);
    
    /// <summary>
    /// Updates an existing healthcare offer.
    /// If file is provided, replaces the existing file.
    /// </summary>
    Task<HealthcareOfferDto> UpdateAsync(Guid id, UpdateHealthcareOfferDto input);
    
    /// <summary>
    /// Deletes a healthcare offer and its associated file.
    /// </summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Downloads a healthcare offer file (employee view).
    /// </summary>
    Task<Stream> DownloadAsync(Guid id);
}
