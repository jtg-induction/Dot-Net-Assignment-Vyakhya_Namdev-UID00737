using DotNetRestaurantManagement.Filters;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
using DotNetRestaurantManagement.Constants;
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
        public async Task<IHttpActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            long userId = ClaimsHelper.GetUserId(User);
            var result = await _orderService.PlaceOrderAsync(userId, request);
            return Created("", new ApiResponse<OrderResponse>(true, result, SuccessMessages.OrderPlaced));
        }
    }
}
