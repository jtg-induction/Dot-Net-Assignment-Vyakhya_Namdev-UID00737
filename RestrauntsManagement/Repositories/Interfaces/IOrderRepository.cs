using DotNetRestaurantManagement.Models.Entities;
using System.Collections.Generic;
using DotNetRestaurantManagement.Models.DTO;
using System.Data;
using System.Data.Entity;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        ITransaction BeginTransaction(IsolationLevel isolationLevel);

        Task<bool> DoesAddressBelongsToUserAsync(
            long userId,
            long addressId);

        Task<List<MenuItem>> GetMenuItemsAsync(
            IEnumerable<long> menuItemIds,
            long restaurantId);

        Task<Order> GetOrderAsync(
            long orderId,
            long userId);

        void AddOrder(Order order);

        void AddOrderItem(OrderItem orderItem);

        Task<OrderDetailsResponse> GetOrderDetailsAsync(long orderId, long userId);
        Task SaveChangesAsync();
    }
}
