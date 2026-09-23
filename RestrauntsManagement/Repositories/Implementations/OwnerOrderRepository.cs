using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories.Interfaces;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Implementations
{
    public class OwnerOrderRepository : IOwnerOrderRepository
    {
        private readonly RestaurantDbContext _context;
        public OwnerOrderRepository(RestaurantDbContext context)
        {
            _context = context;
        }
        public IQueryable<Order> GetOrders(long ownerId)
        {
            return _context.Orders
                    .Where(order => order.Restaurant.OwnerId == ownerId);
        }

        public async Task<Order> GetOrderForStatusUpdateAsync(
            long ownerId,
            long restaurantId,
            long orderId)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(order =>
                    order.Id == orderId &&
                    order.RestaurantId == restaurantId &&
                    order.Restaurant.OwnerId == ownerId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
