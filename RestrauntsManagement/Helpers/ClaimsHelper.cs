using DotNetRestaurantManagement.Constants;
using System;
using System.Security.Claims;
using System.Security.Principal;

namespace DotNetRestaurantManagement.Helpers
{
    /// <summary>
    /// Provides helper methods for reading user information from JWT claims
    /// </summary>
    public static class ClaimsHelper
    {
        /// <summary>
        /// Gets the user ID from the authenticated user's claims
        /// </summary>
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

        /// <summary>
        /// Gets the user's role from the authenticated user's claims
        /// </summary>
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

        /// <summary>
        /// Gets the refresh token ID from the authenticated user's claims
        /// </summary>
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
