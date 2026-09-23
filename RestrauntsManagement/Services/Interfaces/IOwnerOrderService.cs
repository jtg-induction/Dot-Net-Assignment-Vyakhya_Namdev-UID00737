using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Interfaces
{
    public interface IOwnerOrderService
    {
        Task<PaginationResult<DashboardOrderResponse>> GetOrdersAsync(long ownerId, DashboardOrderRequest request);
        Task<UpdateOrderStatus> UpdateOrderStatusAsync(
            long ownerId,
            long restaurantId,
            long orderId,
            UpdateOrderStatus request);
    }
}
