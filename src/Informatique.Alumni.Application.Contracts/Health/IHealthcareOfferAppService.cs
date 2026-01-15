using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Health;

/// <summary>
/// Application service interface for healthcare offers (graduate portal).
/// Business Rule: Only graduates with active membership can access.
/// Clean Code: Read-only interface - no Create/Update/Delete methods.
/// </summary>
public interface IHealthcareOfferAppService : IApplicationService
{
    /// <summary>
    /// Gets the list of healthcare offers for graduates with pagination.
    /// Business Rule: Requires active membership - throws UserFriendlyException if inactive.
    /// </summary>
    Task<PagedResultDto<HealthcareOfferListDto>> GetListAsync(GetHealthcareOffersInput input);
    
    /// <summary>
    /// Downloads a healthcare offer file.
    /// Business Rule: Requires active membership.
    /// </summary>
    Task<Stream> DownloadAsync(Guid id);
}
