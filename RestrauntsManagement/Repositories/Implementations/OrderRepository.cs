using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.DTO;
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

        public async Task<OrderDetailsResponse> GetOrderDetailsAsync(
            long orderId, long userId)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Where(x => x.Id == orderId && x.CustomerId == userId)
                .Select(x => new
                {
                    x.Id,
                    x.DeliveryAddressId,
                    x.TotalAmount,
                    x.Status,
                    x.RestaurantId,

                    Items = x.OrderedItems.Select(
                        o => new
                        {
                            o.MenuItemId,
                            o.Price,
                            o.Quantity
                        })
                })
                .FirstOrDefaultAsync();

            if (order == null) return null;
            return new OrderDetailsResponse
            {
                OrderId = orderId,
                DeliveryAddress = order.DeliveryAddressId,
                TotalAmount = order.TotalAmount,
                TotalItems = order.Items.Sum(x => x.Quantity),
                OrderStatus = order.Status,
                RestaurantId = order.RestaurantId,
                Items = order.Items
                   .Select(x => new OrderItemResponse
                   {
                       MenuItemId = x.MenuItemId,
                       Price = x.Price,
                       Quantity = x.Quantity
                   })
                   .ToList()
            };
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
