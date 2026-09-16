using DotNetRestaurantManagement.Models.DTO;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponse> PlaceOrderAsync(long userId, PlaceOrderRequest request);
        Task<OrderDetailsResponse> GetOrderDetailsAsync(long orderId, long userId);
    }
}
