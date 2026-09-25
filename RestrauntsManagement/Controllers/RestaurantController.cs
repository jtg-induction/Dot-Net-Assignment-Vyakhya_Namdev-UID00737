using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Filters;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Services.Interfaces;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace DotNetRestaurantManagement.Controllers
{
    [RoutePrefix("api/restaurants")]
    public class RestaurantController : ApiController
    {
        private readonly IRestaurantService _restaurantService;
        public RestaurantController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        /// To get the details of all restaurants
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetRestaurants([FromUri] PaginationRequest request)
        {
            request = request ?? new PaginationRequest();
            var result = await _restaurantService.GetRestaurantsDetailsAsync(request);
            return Ok(new ApiResponse<PaginationResult<RestaurantDto>>(true, result, SuccessMessages.RestaurantsListed));
        }

        /// To get the restaurants wise menu
        [HttpGet]
        [Route("{restaurantId:long}/menu")]
        public async Task<IHttpActionResult> GetRestaurantMenu(long restaurantId, [FromUri] PaginationRequest request)
        {
            request = request ?? new PaginationRequest();
            if (restaurantId <= 0) return BadRequest(ErrorMessages.InvalidRestaurantId);
            var result = await _restaurantService.GetRestaurantMenuAsync(restaurantId, request);
            return Ok(new ApiResponse<PaginationResult<MenuItemDto>>(true, result, SuccessMessages.RestaurantMenuListed));
        }

        /// Onboard Restaurant
        [JwtAuthorize]
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> OnboardRestaurant(OnboardRestaurantRequest request)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var role = ClaimsHelper.GetUserRole(User);
            if (role != UserRole.SuperAdmin.ToString())
            {
                throw new ApiException(HttpStatusCode.Forbidden, ErrorMessages.NotAuthenticated);
            }
            var response = await _restaurantService.OnboardRestaurant(request);
            return Created("", new ApiResponse<OnboardRestaurantResponse>(true, response, SuccessMessages.RestaurantOnboarded));
        }
    }
}
