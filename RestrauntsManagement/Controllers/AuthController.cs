using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
using System;
using System.Net;
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

        [HttpPost]
        [Route("login")]
        public async Task<IHttpActionResult> Login(LoginRequest request)
        {
            if (request == null) return BadRequest("Request body is required!");
            if (!ModelState.IsValid) return BadRequest(ModelState); 
            try
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
            catch (InvalidCredentialsException)
            {
                return Unauthorized();
            }
            catch (Exception)
            {
                return InternalServerError();
            }

        }
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

        [HttpPost]
        [Route("refresh-token")]
        public async Task<IHttpActionResult> RefreshToken()
        {
            var cookie = HttpContext.Current.Request.Cookies["RefreshToken"];
            if (cookie == null){
                return Unauthorized();
            }

            var response = await _authService.RefreshTokenAsync(cookie.Value);
            SetCookie("AccessToken", response.AccessToken, DateTime.UtcNow.AddMinutes(15));
            SetCookie("RefreshToken", response.RefreshToken, DateTime.UtcNow.AddDays(7));
            return Ok(new
            {
                Message = "Token refreshed successfully!"
            });
        }

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
    }
}
