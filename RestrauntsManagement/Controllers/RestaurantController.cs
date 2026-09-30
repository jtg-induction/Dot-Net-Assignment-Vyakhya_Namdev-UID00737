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

        /// <summary>
        /// Gets a paginated list of all active restaurants
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetRestaurants([FromUri] PaginationRequest request)
        {
            request = request ?? new PaginationRequest();
            var result = await _restaurantService.GetRestaurantsDetailsAsync(request);
            var response = new RestaurantPaginationResult
            {
                Restaurants = result.Items,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages,
                HasPreviousPage = result.HasPreviousPage,
                HasNextPage = result.HasNextPage
            };
            return Ok(new ApiResponse<RestaurantPaginationResult>(true, response, SuccessMessages.RestaurantsListed));
        }

        /// <summary>
        /// Gets the paginated menu items available at a specific restaurant
        /// </summary>
        [HttpGet]
        [Route("{restaurantId}/menu")]
        public async Task<IHttpActionResult> GetRestaurantMenu(long restaurantId, [FromUri] PaginationRequest request)
        {
            request = request ?? new PaginationRequest();
            if (restaurantId <= 0) return BadRequest(ErrorMessages.InvalidRestaurantId);
            var result = await _restaurantService.GetRestaurantMenuAsync(restaurantId, request);
            var response = new MenuPaginationResult
            {
                MenuItems = result.Items,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages,
                HasPreviousPage = result.HasPreviousPage,
                HasNextPage = result.HasNextPage
            };
            return Ok(new ApiResponse<MenuPaginationResult>(true, response, SuccessMessages.RestaurantMenuListed));
        }

        /// <summary>
        /// Adds a new restaurant to the system
        /// </summary>
        [JwtAuthorize(UserRole.SuperAdmin)]
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> OnboardRestaurant([FromBody] OnboardRestaurantRequest request)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var response = await _restaurantService.OnboardRestaurant(request);
            return Created("", new ApiResponse<OnboardRestaurantResponse>(true, response, SuccessMessages.RestaurantOnboarded));
        }
    }
}
