using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Filters;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Services.Interfaces;
using System.Threading.Tasks;
using System.Web.Http;

namespace DotNetRestaurantManagement.Controllers
{
    [RoutePrefix("api/owner/restaurants")]
    public class OwnerOrderController : ApiController
    {
        private readonly IOwnerOrderService _orderService;
        public OwnerOrderController(IOwnerOrderService orderService)
        {
            _orderService = orderService;
        }

        [JwtAuthorize]
        [RoleAuthorize(UserRole.Owner)]
        [HttpGet]
        [Route("orders")]
        public async Task<IHttpActionResult> GetOrders([FromUri] DashboardOrderRequest request)
        {
            if (request == null)
            {
                request = new DashboardOrderRequest();
            }
            long userId = ClaimsHelper.GetUserId(User);
            var response = await _orderService.GetOrdersAsync(userId, request);
            return Ok(new ApiResponse<object>(
                true,
                response,
                SuccessMessages.OrdersFetched));
        }

        [JwtAuthorize]
        [RoleAuthorize(UserRole.Owner)]
        [HttpPatch]
        [Route("{restaurantId:long}/orders/{orderId:long}")]
        public async Task<IHttpActionResult> UpdateOrderStatus(
            long restaurantId,
            long orderId,
            UpdateOrderStatus request)
        {
            long ownerId = ClaimsHelper.GetUserId(User);
            var response = await _orderService.UpdateOrderStatusAsync(
                            ownerId,
                            restaurantId,
                            orderId,
                            request);

            return Ok(new ApiResponse<object>(
                true,
                response,
                SuccessMessages.OrderStatusUpdated));
        }
    }
}
