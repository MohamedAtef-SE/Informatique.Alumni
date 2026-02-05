using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.Identity;

namespace Informatique.Alumni.Users;

public class UserCreatedEventHandler : ILocalEventHandler<EntityCreatedEventData<IdentityUser>>, ITransientDependency
{
    private readonly IdentityUserManager _userManager;

    public UserCreatedEventHandler(IdentityUserManager userManager)
    {
        _userManager = userManager;
    }

    public async Task HandleEventAsync(EntityCreatedEventData<IdentityUser> eventData)
    {
        var user = eventData.Entity;
        
        // Helper method to safely assign role
        await AssignAlumniRoleAsync(user);
    }

    private async Task AssignAlumniRoleAsync(IdentityUser user)
    {
        // Avoid adding role to admin if undesirable, but user requested "active alumni"
        // Usually, 'admin' user might also want alumni permissions for testing.
        // We will add 'alumni' role to EVERYONE to be safe and meet the requirement "Active alumni they should have".
        
        // Check if role exists is handled by UserManager usually, but we assume "alumni" role exists (seeded)
        const string roleName = "alumni";
        
        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }
    }
}
