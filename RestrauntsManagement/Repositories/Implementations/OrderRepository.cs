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

        public async Task<Order> GetOrderAsync(
                long orderId,
                long userId)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(x => x.Id == orderId && 
                x.CustomerId == userId);
        }
        public void AddOrder(Order order)
        {
            _context.Orders.Add(order);
        }

        public void AddOrderItem(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
        }

        public async Task<OrderDetailsResponse> GetOrderDetailsAsync(
                long orderId,
                long userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(x =>
                    x.Id == orderId &&
                    x.CustomerId == userId)
                .Select(x => new OrderDetailsResponse
                {
                    OrderId = x.Id,
                    Items = x.OrderedItems
                        .Select(o => new OrderItemResponse
                        {
                            MenuItemId = o.MenuItemId,
                            Quantity = o.Quantity,
                            Price = o.Price
                        })
                        .ToList(),

                    DeliveryAddress = new AddressResponse
                    {
                        Id = x.DeliveryAddress.Id,
                        HouseNumber = x.DeliveryAddress.HouseNumber,
                        StreetAddress = x.DeliveryAddress.StreetAddress,
                        City = x.DeliveryAddress.City,
                        State = x.DeliveryAddress.State,
                        PinCode = x.DeliveryAddress.PinCode,
                        Country = x.DeliveryAddress.Country,
                        AddressType = (int)x.DeliveryAddress.AddressType
                    },

                    TotalAmount = x.TotalAmount,
                    TotalItems = x.TotalItems,
                    OrderStatus = x.Status,
                    Restaurant = new RestaurantResponse
                    {
                        RestaurantId = x.Restaurant.Id,
                        Name = x.Restaurant.Name,
                        Email = x.Restaurant.Email,
                        Cuisine = x.Restaurant.Cuisine.ToString(),

                        Address = new RestaurantAddressDto
                        {
                            HouseNumber = x.Restaurant.Address.HouseNumber,
                            StreetAddress = x.Restaurant.Address.StreetAddress,
                            City = x.Restaurant.Address.City,
                            State = x.Restaurant.Address.State,
                            PinCode = x.Restaurant.Address.PinCode,
                            Country = x.Restaurant.Address.Country
                        }
                    }
                })
                .FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
