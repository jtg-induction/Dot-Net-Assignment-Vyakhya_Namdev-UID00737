using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
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
        public async Task<IHttpActionResult> Signup(SignupRequest request){
            SignupResponse response = await _authService.Signup(request);
            return Content(HttpStatusCode.Created, response);
        }
    }
}
