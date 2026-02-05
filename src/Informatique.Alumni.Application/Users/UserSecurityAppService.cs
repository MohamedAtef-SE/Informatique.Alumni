using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Informatique.Alumni.Permissions;

namespace Informatique.Alumni.Users;

[Authorize]
public class UserSecurityAppService : ApplicationService, IUserSecurityAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityUserRepository _userRepository;

    public UserSecurityAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository)
    {
        _userManager = userManager;
        _userRepository = userRepository;
    }

    [Authorize(AlumniPermissions.Users.Manage)] // Assuming generic 'Manage Users' permission or specific
    public async Task ChangePasswordForUserAsync(AdminChangePasswordDto input)
    {
        var user = await _userManager.FindByIdAsync(input.UserId.ToString());
        if (user == null)
        {
            throw new UserFriendlyException("User not found.");
        }

        // 1. Check Branch Permissions (Isolation)
        if (!CurrentUser.IsInRole("admin"))
        {
            var adminBranchIdClaim = CurrentUser.FindClaim("BranchId");
            if (adminBranchIdClaim != null && Guid.TryParse(adminBranchIdClaim.Value, out var adminBranchId))
            {
                var userBranchId = user.GetProperty<Guid?>("BranchId");
                
                // If user belongs to a branch and it's different -> Security Error
                // Note: If userBranchId is null, and admin works for a branch, can they edit? 
                // Strict rule: "Admin can ONLY change passwords for users in *their* Branch."
                // Implies if user has NO branch, they ARE NOT in *their* branch.
                if (userBranchId != adminBranchId)
                {
                    throw new UserFriendlyException("Security Error: You cannot manage users from other branches.");
                }
            }
            else
            {
                // If Admin has no branch (and not SuperAdmin), they shouldn't edit anyone? 
                // Or maybe they are global but limited role?
                // Adhering to strict rule: No Branch Claim = No Access to Branch Users? 
                // Assume safe default for this specific task context: If not 'admin' role, assume restricted.
                // But if FindClaim returns null, we might be in an inconsistent state or user is generic.
                // Let's assume strictness.
                throw new UserFriendlyException("Security Error: Your account is not associated with a branch.");
            }
        }

        // 2. History Check (New != Old)
        if (await _userManager.CheckPasswordAsync(user, input.NewPassword))
        {
             throw new UserFriendlyException("New password cannot be the same as the old password.");
        }

        // 3. Force Change (Remove + Add to bypass Token requirement if Token provider setup is complex, 
        // OR Generate Token -> Reset. 
        // Token is cleaner for auditing, but Remove+Add is robust admin action.
        // I will use Token for correctness with ABP/Identity.
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, input.NewPassword);

        if (!result.Succeeded)
        {
             throw new UserFriendlyException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        
        // 4. Notification (Optional per requirements, skipping for now or adding stub)
    }

    public async Task ChangeMyPasswordAsync(UserChangePasswordDto input)
    {
        var user = await _userManager.GetByIdAsync(CurrentUser.GetId());
        
        var result = await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
        
        if (!result.Succeeded)
        {
             throw new UserFriendlyException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
