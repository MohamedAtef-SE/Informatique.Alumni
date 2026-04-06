using System;
using System.Threading.Tasks;
using Informatique.Alumni.Localization;
using Informatique.Alumni.Profiles;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni;

/* Inherit your application services from this class.
 */
public abstract class AlumniAppService : ApplicationService
{
    // Property injection supported natively by Autofac for ABP Base Classes
    public IRepository<AlumniProfile, Guid> BaseAlumniProfileRepository { get; set; }

    protected AlumniAppService()
    {
        LocalizationResource = typeof(AlumniResource);
    }

    /// <summary>
    /// Safely resolves the active physical Profile.Id from the generic HTTP Identity session.
    /// Definitively eliminates structural Domain anomalies globally (e.g. Alumni:Profile:009).
    /// </summary>
    protected virtual async Task<Guid> GetCurrentAlumniProfileIdAsync()
    {
        var currentUserId = CurrentUser.Id;
        if (!currentUserId.HasValue)
        {
            throw new AbpAuthorizationException("User must be logged in.");
        }

        var profile = await BaseAlumniProfileRepository.FirstOrDefaultAsync(p => p.UserId == currentUserId.Value);
        if (profile == null)
        {
            throw new UserFriendlyException("Alumni profile not found for the current user.");
        }

        return profile.Id;
    }
}
