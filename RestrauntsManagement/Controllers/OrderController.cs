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

        /// Places a new order for the current user.
        [JwtAuthorize]
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> PlaceOrder(PlaceOrderRequest request)
        {
            long userId = ClaimsHelper.GetUserId(User);
            long orderId = await _orderService.PlaceOrderAsync(userId, request);
            return Ok(new ApiResponse<long>(true, orderId));
        }

        /// Fetching the order details made by user
        [JwtAuthorize]
        [HttpGet]
        [Route("{orderId:long}")]
        public async Task<IHttpActionResult> GetOrderDetails(long orderId)
        {
            long userId = ClaimsHelper.GetUserId(User);
            var order = await _orderService
                .GetOrderDetailsAsync(orderId, userId);
            if(order == null)
            {
                throw new ApiException(
                    HttpStatusCode.NotFound, ErrorMessages.OrderNotFound
                    );
            }
            return Ok(new ApiResponse<OrderDetailsResponse>
            (
                true,
                order,
                SuccessMessages.OrderSuccessMessage
            ));
        }

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
