using DotNetRestaurantManagement.Filters;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
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
        [HttpPut]
        [Route("update-profile")]
        public async Task<IHttpActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            long userId = ClaimsHelper.GetUserId(User);
            var response = await _userService.UpdateProfileAsync(userId, request);
            return Ok(new ApiResponse<UpdateProfileResponse>(true, response));
        }

        /// Changes the current user's password.
        [JwtAuthorize]
        [HttpPut]
        [Route("update-password")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordRequest request)
        {
            long userId = ClaimsHelper.GetUserId(User);
            await _userService.ChangePasswordAsync(userId, request);
            return Ok(true);
        }

        /// Adds an address for the current user.
        [JwtAuthorize]
        [HttpPost]
        [Route("update-address")]
        public async Task<IHttpActionResult> AddAddress(AddressRequest request)
        {
            long userId = ClaimsHelper.GetUserId(User);
            var response = await _userService.AddAddressAsync(userId, request);
            return Content(System.Net.HttpStatusCode.Created, response);
        }
    }
}
