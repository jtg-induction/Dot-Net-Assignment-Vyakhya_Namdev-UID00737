using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
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

        /// To get the deatils of all restaurants
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetRestaurants([FromUri] PaginationRequest request)
        {
            request = request ?? new PaginationRequest();
            var result = await _restaurantService.GetPaginationResultAsync(request);
            return Ok(new ApiResponse<PaginationResult<RestaurantDto>>(true, result));
        }

        /// To get the restaurants wise menu
        [HttpGet]
        [Route("{restaurantId:long}/menu")]
        public async Task<IHttpActionResult> GetRestaurantMenu(long restaurantId, [FromUri] PaginationRequest request)
        {
            request = request ?? new PaginationRequest();
            if (restaurantId <= 0) return BadRequest("RestaurantId must be greater than 0!");
            var result = await _restaurantService.GetRestaurantMenuAsync(restaurantId, request);
            return Ok(new ApiResponse<PaginationResult<MenuItemDto>>(true, result));
        }
    }
}
