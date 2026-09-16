using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using System.Data;
using System.Data.Entity;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        ITransaction BeginTransaction(IsolationLevel isolationLevel);

        Task<bool> IsAddressBelongsToUserAsync(
            long userId,
            long addressId);

        Task<MenuItem> GetMenuItemAsync(
            long menuItemId,
            long restaurantId);

        void AddOrder(Order order);

        void AddOrderItem(OrderItem orderItem);

        Task<OrderDetailsResponse> GetOrderDetailsAsync(long orderId, long userId);

        Task SaveChangesAsync();
    }
}
