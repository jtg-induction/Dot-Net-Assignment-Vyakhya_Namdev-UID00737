using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Filters;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace DotNetRestaurantManagement.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// Updates the current user's profile.
        [JwtAuthorize]
        [HttpPatch]
        [Route("profile")]
        public async Task<IHttpActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            long userId = ClaimsHelper.GetUserId(User);
            var response = await _userService.UpdateProfileAsync(userId, request);
            return Ok(new ApiResponse<UpdateProfileResponse>(true, response, SuccessMessages.UserProfileUpdated));
        }

        /// Changes the current user's password.
        [JwtAuthorize]
        [HttpPut]
        [Route("password")]
        public async Task<IHttpActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            long userId = ClaimsHelper.GetUserId(User);
            await _userService.ChangePasswordAsync(userId, request);
            return Ok(new ApiResponse<object>(true, SuccessMessages.PasswordUpdated));
        }

        /// Deactivates the user's account from all devices and market it as in-active user
        [JwtAuthorize]
        [HttpPut]
        [Route("deactivate")]
        public async Task<IHttpActionResult> DeactivateAccount()
        {
            long userId = ClaimsHelper.GetUserId(User);
            long refreshTokenId = ClaimsHelper.GetRefreshTokenId(User);
            await _userService.DeactivateAccountAsync(userId, refreshTokenId);
            return Ok(new ApiResponse<object>(true, SuccessMessages.AccountDeactivated));
        }
    }
}
