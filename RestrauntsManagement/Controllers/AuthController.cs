using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Filters;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace DotNetRestaurantManagement.Controllers
{
    // Handles authentication-related API endpoints.
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // <summary>
        // Creates a new user account.
        // </summary>
        // <param name="request">The signup details provided by the user.</param>
        // <returns>A response containing the newly created account details.</returns>
        [HttpPost]
        [Route("signup")]
        public async Task<IHttpActionResult> Signup([FromBody] SignupRequest request){
            SignupResponse response = await _authService.Signup(request);
            return Created("",new ApiResponse<SignupResponse>(true, response, SuccessMessages.UserCreated));
        }

        /// <summary>
        /// Authenticates a user and creates access and refresh tokens.
        /// </summary>
        /// <param name="request">The user's login credentials.</param>
        /// <returns>The authenticated user's basic details.</returns>
        [HttpPost]
        [Route("login")]
        public async Task<IHttpActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(new ApiResponse<LoginResponse>(true, response, SuccessMessages.UserLoggedIn));
        }

        /// Generates new access and refresh tokens using the refresh token cookie.
        /// A response indicating that the tokens were refreshed successfully.
        [HttpPost]
        [Route("refresh-token")]
        public async Task<IHttpActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(new ApiResponse<RefreshTokenResponse>(true, response,SuccessMessages.TokenRefreshed));
        }

        /// Logs out the current user and removes the authentication cookies.
        /// A response indicating that the user was logged out successfully.
        [HttpPost]
        [Route("logout")]
        public async Task<IHttpActionResult> Logout()
        {
            var userId = ClaimsHelper.GetUserId(User);
            var refreshTokenId = ClaimsHelper.GetRefreshTokenId(User);
            await _authService.LogoutAsync(userId, refreshTokenId);

            return Ok(new ApiResponse<object>(true, SuccessMessages.LogoutSuccessMessage));
        }
    }
}
