using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Informatique.Alumni.Profiles;

namespace Informatique.Alumni.Membership;

/// <summary>
/// Reusable service to enforce "Active Membership" guard across multiple AppServices.
/// </summary>
public class MembershipGuard : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly MembershipManager _membershipManager;

    public MembershipGuard(
        ICurrentUser currentUser,
        IRepository<AlumniProfile, Guid> profileRepository,
        MembershipManager membershipManager)
    {
        _currentUser = currentUser;
        _profileRepository = profileRepository;
        _membershipManager = membershipManager;
    }

    /// <summary>
    /// Checks if the current user has an active membership.
    /// Throws UserFriendlyException if not valid.
    /// </summary>
    public async Task CheckAsync()
    {
        var currentUserId = _currentUser.Id;
        if (currentUserId == null)
        {
             throw new UserFriendlyException("User not authenticated.");
        }

        // Using FindAsync with predicate for cleaner syntax
        var profile = await _profileRepository.FindAsync(p => p.UserId == currentUserId.Value);

        if (profile == null)
        {
             throw new UserFriendlyException("Access denied. Active membership required.");
        }

        var isActive = await _membershipManager.IsActiveAsync(profile.Id);
        if (!isActive)
        {
            throw new UserFriendlyException("Access denied. Active membership required.");
        }
    }
}
