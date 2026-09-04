using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
using DotNetRestaurantManagement.Exceptions;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.IdentityModel.Tokens.Jwt;

namespace DotNetRestaurantManagement.Controllers
{
    // Handles authentication-related API endpoints.
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        // <summary>
        // Creates a new user account.
        // </summary>
        // <param name="request">The signup details provided by the user.</param>
        // <returns>A response containing the newly created account details.</returns>
        [HttpPost]
        [Route("signup")]
        public async Task<IHttpActionResult> Signup(SignupRequest request){
            SignupResponse response = await _authService.Signup(request);
            return Content(HttpStatusCode.Created, response);
        }

        /// <summary>
        /// Authenticates a user and creates access and refresh tokens.
        /// </summary>
        /// <param name="request">The user's login credentials.</param>
        /// <returns>The authenticated user's basic details.</returns>
        [HttpPost]
        [Route("login")]
        public async Task<IHttpActionResult> Login(LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            SetCookie("AccessToken", response.AccessToken, DateTime.UtcNow.AddMinutes(15));
            SetCookie("RefreshToken", response.RefreshToken, DateTime.UtcNow.AddDays(7));
            return Ok(new
            {
                response.Name,
                response.Email,
                Message = "Login Successful!"
            });
        }

        /// This sets the token in cookies
        private void SetCookie(string key, string value, DateTime expires)
        {
            var cookie = new HttpCookie(key)
            {
                Value = value,
                HttpOnly = true,
                Secure = true,
                Expires = expires,
                SameSite = SameSiteMode.Strict
            };

            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// This deleted the token from cookies
        private void DeleteCookie(string key)
        {
            var cookie = new HttpCookie(key)
            {
                Value = string.Empty,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1)
            };

            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// Generates new access and refresh tokens using the refresh token cookie.
        /// A response indicating that the tokens were refreshed successfully.
        [HttpPost]
        [Route("refresh-token")]
        public async Task<IHttpActionResult> RefreshToken()
        {
            var cookie = HttpContext.Current.Request.Cookies["RefreshToken"];
            if (cookie == null){
                throw new InvalidRefreshTokenException();
            }

            var response = await _authService.RefreshTokenAsync(cookie.Value);
            SetCookie("AccessToken", response.AccessToken, DateTime.UtcNow.AddMinutes(15));
            SetCookie("RefreshToken", response.RefreshToken, DateTime.UtcNow.AddDays(7));
            return Ok(new
            {
                Message = "Token refreshed successfully!"
            });
        }

        /// Logs out the current user and removes the authentication cookies.
        /// A response indicating that the user was logged out successfully.
        [HttpPost]
        [Route("logout")]
        public async Task<IHttpActionResult> Logout()
        {
            var cookie = HttpContext.Current.Request.Cookies["RefreshToken"];
            if (cookie == null) throw new InvalidRefreshTokenException();
            await _authService.LogoutAsync(cookie.Value);

            DeleteCookie("AccessToken");
            DeleteCookie("RefreshToken");

            return Ok(new
            {
                Message = "Logged out successfully!"
            });
        }

        [HttpPut]
        [Route("update-profile")]
        public async Task<IHttpActionResult> UpdateProfile(
    UpdateProfileRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required!");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int userId = GetUserIdFromClaims();

                var response =
                    await _authService.UpdateProfileAsync(
                        userId,
                        request);

                return Ok(response);
            }
            catch (DuplicateEmailException ex)
            {
                return Content(
                    HttpStatusCode.Conflict,
                    new ErrorResponse { Message = ex.Message });
            }
            catch (DuplicatePhoneNumberException ex)
            {
                return Content(
                    HttpStatusCode.Conflict,
                    new ErrorResponse { Message = ex.Message });
            }
            catch (PhoneNumberAlreadyRegistered ex)
            {
                return Content(
                    HttpStatusCode.Conflict,
                    new ErrorResponse { Message = ex.Message });
            }
            catch (UserNotFound)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        private int GetUserIdFromClaims()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var userIdClaim =
                claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)
                ?? claimsIdentity?.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException();
            }

            return userId;
        }

        [HttpPut]
        [Route("update-password")]
        public async Task<IHttpActionResult> ChangePassword(
    ChangePasswordRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required!");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int userId = GetUserIdFromClaims();

                await _authService.ChangePasswordAsync(
                    userId,
                    request);

                return Ok(new
                {
                    Message = "Password updated successfully!"
                });
            }
            catch (InvalidCredentialsException)
            {
                return Unauthorized();
            }
            catch (UserNotFound)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [HttpPost]
        [Route("update-address")]
        public async Task<IHttpActionResult> AddAddress(
    AddressRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required!");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                int userId = GetUserIdFromClaims();

                var response =
                    await _authService.AddAddressAsync(
                        userId,
                        request);

                return Content(
                    HttpStatusCode.Created,
                    response);
            }
            catch (UserNotFound)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }
    }
}
