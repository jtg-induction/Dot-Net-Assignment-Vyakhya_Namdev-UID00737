using DotNetRestaurantManagement.Constants;
using System;
using System.Security.Claims;
using System.Security.Principal;

namespace DotNetRestaurantManagement.Helpers
{
    public static class ClaimsHelper
    {
        public static long GetUserId(IPrincipal user)
        {
            var claimsPrincipal = user as ClaimsPrincipal;
            var claim = claimsPrincipal?.FindFirst(StringConstants.UserId);
            if (claim == null || !long.TryParse(claim.Value, out var userId))
            {
                throw new UnauthorizedAccessException(ErrorMessages.UserIdMissingClaim);
            }

            return userId;
        }

        public static string GetUserRole(IPrincipal user)
        {
            var claimsPrincipal = user as ClaimsPrincipal;
            var claim = claimsPrincipal?.FindFirst(ClaimTypes.Role);
            if (claim == null || string.IsNullOrWhiteSpace(claim.Value))
            {
                throw new UnauthorizedAccessException(ErrorMessages.RoleMissingClaim);
            }

            return claim.Value;
        }

        public static long GetRefreshTokenId(IPrincipal user)
        {
            var claimsPrincipal = user as ClaimsPrincipal;
            var claim = claimsPrincipal?.FindFirst(StringConstants.RefreshTokenId);
            if (claim == null || !long.TryParse(claim.Value, out var refreshTokenId))
            {
                throw new UnauthorizedAccessException(ErrorMessages.RefreshTokenIdMissingClaim);
            }
            return refreshTokenId;
        }
    }
}
