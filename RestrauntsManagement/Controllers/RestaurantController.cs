using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
using System;
using System.Collections.Generic;
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

        /// Get: api/restaurants?page=1&pageSize=10
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetRestaurants([FromUri] PaginationRequest request)
        {
            try
            {
                request = request ?? new PaginationRequest();
                var result = await _restaurantService.GetPaginationResultAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("{restaurantId:long}/menu")]
        public async Task<IHttpActionResult> GetRestaurantMenu(long restaurantId, [FromUri] PaginationRequest request)
        {
            try
            {
                request = request ?? new PaginationRequest();
                if (restaurantId <= 0) return BadRequest("RestaurantId must be greater than 0!");
                var result = await _restaurantService.GetRestaurantMenuAsync(restaurantId, request);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
