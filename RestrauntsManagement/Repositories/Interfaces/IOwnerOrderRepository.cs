using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Interfaces
{
    public interface IOwnerOrderRepository
    {
        IQueryable<Order> GetOrders(long ownerId);
        Task<Order> GetOrderForStatusUpdateAsync(
            long ownerId,
            long restaurantId,
            long orderId);
        Task SaveChangesAsync();
    }
}
