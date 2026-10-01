using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Filters;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace DotNetRestaurantManagement.Controllers
{
    [RoutePrefix("api/orders")]
    public class OrderController : ApiController
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Places a new order for the currently logged-in user
        /// </summary>
        [JwtAuthorize]
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            long userId = ClaimsHelper.GetUserId(User);
            var result = await _orderService.PlaceOrderAsync(userId, request);
            return Created("", new ApiResponse<OrderResponse>(true, result, SuccessMessages.OrderPlaced));
        }

        /// <summary>
        /// Gets the details of a specific order placed by the currently logged-in user
        /// </summary>
        [JwtAuthorize]
        [HttpGet]
        [Route("{orderId:long}")]
        public async Task<IHttpActionResult> GetOrderDetails(long orderId)
        {
            long userId = ClaimsHelper.GetUserId(User);
            var order = await _orderService.GetOrderDetailsAsync(orderId, userId);
            return Ok(new ApiResponse<OrderDetailsResponse>
            (
                true,
                order,
                SuccessMessages.OrderSuccessMessage
            ));
        }

        /// <summary>
        /// Cancels an order placed by the currently logged-in user
        /// </summary>
        [HttpPatch]
        [Route("{orderId:long}")]
        [JwtAuthorize]
        public async Task<IHttpActionResult> CancelOrder(long orderId)
        {
            long userId = ClaimsHelper.GetUserId(User);
            var result = await _orderService.CancelOrderAsync(
                orderId,
                userId);

            return Ok(new ApiResponse<CancelOrderResponse>(
                            true,
                            result,
                            SuccessMessages.OrderCancelled));
        }
    }
}
