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
        /// <summary>
        /// Initializes the owner order repository with the database context
        /// </summary>
        public OwnerOrderRepository(RestaurantDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all orders from restaurants owned by the specified user
        /// </summary>
        public IQueryable<Order> GetOrders(long ownerId)
        {
            return _context.Orders
                    .Where(order => order.Restaurant.OwnerId == ownerId);
        }

        /// <summary>
        /// Gets an order for status update after verifying that it belongs to the specified restaurant and owner
        /// </summary>
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

        /// <summary>
        /// Saves the pending changes to the database
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
