using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly RestaurantDbContext _context;
        public OrderRepository(RestaurantDbContext context)
        {
            _context = context;
        }

        public ITransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            var transaction = _context.Database.BeginTransaction(isolationLevel);
            return new Transaction(transaction);
        }

        public async Task<bool> DoesAddressBelongsToUserAsync(
            long userId,
            long addressId)
        {
            return await _context.UserAddresses
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.AddressId == addressId &&
                    x.IsActive);
        }

        public async Task<List<MenuItem>> GetMenuItemsAsync(
            IEnumerable<long> menuItemIds,
            long restaurantId)
        {
            return await _context.MenuItems
                .Where(x =>
                    x.RestaurantId == restaurantId &&
                    menuItemIds.Contains(x.Id))
                .ToListAsync();
        }

        public void AddOrder(Order order)
        {
            _context.Orders.Add(order);
        }

        public void AddOrderItem(OrderItem orderItem)
        {
            _context.OrderedItems.Add(orderItem);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
