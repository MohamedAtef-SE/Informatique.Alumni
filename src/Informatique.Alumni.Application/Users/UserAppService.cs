using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Informatique.Alumni.Permissions;

namespace Informatique.Alumni.Users;

[Authorize(AlumniPermissions.Users.SystemUsersReport)] // Using existing permission or strictly System Admin one?
// Request says "System Admin - User Management". 
// I should probably use "AlumniPermissions.Users.Default" or similar if exists, or just Authorize.
// Let's use [Authorize] generally or check permissions in methods if granules needed.
// "Branch Admin cannot create a user for a different branch" - this implies logic check.
public class UserAppService : ApplicationService
{
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityUserRepository _userRepository;

    public UserAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository)
    {
        _userManager = userManager;
        _userRepository = userRepository;
    }

    public async Task<PagedResultDto<IdentityUserDto>> GetListAsync(UserFilterDto input)
    {
        // 1. Branch Isolation Logic
        // IF Current User is NOT Super Admin ("admin" role), FORCE BranchId
        if (!CurrentUser.IsInRole("admin"))
        {
            var userBranchIdClaim = CurrentUser.FindClaim("BranchId");
            if (userBranchIdClaim != null && Guid.TryParse(userBranchIdClaim.Value, out var claimBranchId))
            {
                input.BranchId = claimBranchId;
            }
            else
            {
                // If user is not admin AND has no BranchId, they arguably shouldn't see anything or just their own?
                // Rule: "The system MUST FORCE BranchId = CurrentUser.BranchId". 
                // If no branch assigned, maybe empty list?
                return new PagedResultDto<IdentityUserDto>(0, new List<IdentityUserDto>());
            }
        }

        // 2. Query
        var queryable = _userManager.Users;
        
        // Filter by Details
        var query = queryable
            .WhereIf(!string.IsNullOrWhiteSpace(input.FilterText), u => 
                u.UserName.Contains(input.FilterText!) || 
                u.Email.Contains(input.FilterText!)) // Name/Surname not directly on IdentityUser in all versions, wait. IdentityUser HAS Name/Surname.
            .WhereIf(!string.IsNullOrWhiteSpace(input.UserName), u => u.UserName.Contains(input.UserName!))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Email), u => u.Email.Contains(input.Email!))
            .WhereIf(input.IsActive.HasValue, u => u.IsActive == input.IsActive);

        var users = await AsyncExecuter.ToListAsync(query);
        
        // Filter Branch
        if (input.BranchId.HasValue)
        {
            users = users.Where(u => u.GetProperty<Guid?>("BranchId") == input.BranchId).ToList();
        }

        // Total Count
        var totalCount = users.Count;

        // Sort & Page
        // Simple default sort by UserName
        var pagedUsers = users
            .OrderBy(u => u.UserName)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<IdentityUserDto>(
            totalCount,
            ObjectMapper.Map<List<IdentityUser>, List<IdentityUserDto>>(pagedUsers)
        );
    }
}
