using System;
using System.Security.Claims;
using Volo.Abp.Users;

namespace Informatique.Alumni;

public static class AlumniCurrentUserExtensions
{
    public static Guid? GetCollegeId(this ICurrentUser currentUser)
    {
        var claim = currentUser.FindClaim("CollegeId");
        if (claim == null || string.IsNullOrEmpty(claim.Value))
        {
            return null;
        }

        if (Guid.TryParse(claim.Value, out var guid))
        {
            return guid;
        }

        return null;
    }
}
