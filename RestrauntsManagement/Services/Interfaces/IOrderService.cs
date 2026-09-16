using DotNetRestaurantManagement.Models.DTO;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Interfaces
{
    public interface IOrderService
    {
        Task<long> PlaceOrderAsync(long userId, PlaceOrderRequest request);
        Task<OrderDetailsResponse> GetOrderDetailsAsync(long orderId, long userId);
    }
}
