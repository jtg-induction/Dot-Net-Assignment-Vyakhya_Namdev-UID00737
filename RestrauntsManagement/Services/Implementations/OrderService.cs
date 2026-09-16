using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderService(
            IUserRepository userRepository,
            IOrderRepository orderRepository)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
        }

        public async Task<long> PlaceOrderAsync(
            long userId,
            PlaceOrderRequest request)
        {
            if (request == null)
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    ErrorMessages.NullOrderRequestException);
            }

            if (request.Items == null || !request.Items.Any())
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    ErrorMessages.EmptyOrderItemsException);
            }

            if (request.Items.Any(x => x.Quantity <= 0))
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    ErrorMessages.ItemQuantityException);
            }

            if (request.Items
                .GroupBy(x => x.MenuItemId)
                .Any(x => x.Count() > 1))
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    ErrorMessages.DuplicateMenuItemException);
            }

            var restaurantId = request.Items.First().RestaurantId;
            if (request.Items.Any(x => x.RestaurantId != restaurantId))
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    ErrorMessages.ItemsMustBelongToSameRestaurant);
            }

            using (var transaction = _orderRepository.BeginTransaction(
                IsolationLevel.Serializable))
            {
                try
                {
                    var customer = await _userRepository.GetActiveUserAsync(userId);
                    if (customer == null)
                    {
                        throw new ApiException(
                            HttpStatusCode.Unauthorized,
                            ErrorMessages.UserNotFound);
                    }

                    var addressExists = await _orderRepository.IsAddressBelongsToUserAsync(
                                                                userId,
                                                                request.DeliveryAddressId);

                    if (!addressExists)
                    {
                        throw new ApiException(
                            HttpStatusCode.BadRequest,
                            ErrorMessages.AddressNotFound);
                    }

                    decimal totalAmount = 0;
                    long totalItems = 0;
                    var menuItems = new List<MenuItem>();
                    foreach (var requestedItem in request.Items)
                    {
                        var menuItem = await _orderRepository.GetMenuItemAsync(
                                                        requestedItem.MenuItemId,
                                                        restaurantId);

                        if (menuItem == null)
                        {
                            throw new ApiException(
                                HttpStatusCode.NotFound,
                                ErrorMessages.ItemNotFound);
                        }

                        if (menuItem.QuantityAvailable < requestedItem.Quantity)
                        {
                            throw new ApiException(
                                HttpStatusCode.BadRequest,
                                ErrorMessages.InsufficientQuanityError);
                        }

                        totalAmount += menuItem.Price * requestedItem.Quantity;
                        totalItems += requestedItem.Quantity;
                        menuItems.Add(menuItem);
                    }

                    if (customer.Balance < totalAmount)
                    {
                        throw new ApiException(
                            HttpStatusCode.BadRequest,
                            ErrorMessages.InsufficientBalance);
                    }

                    customer.Balance -= totalAmount;
                    foreach (var menuItem in menuItems)
                    {
                        var requestedItem = request.Items.First(
                            x => x.MenuItemId == menuItem.Id);

                        menuItem.QuantityAvailable -= requestedItem.Quantity;
                    }

                    var order = new Order
                    {
                        CustomerId = userId,
                        RestaurantId = restaurantId,
                        DeliveryAddressId = request.DeliveryAddressId,
                        TotalItems = totalItems,
                        TotalAmount = totalAmount,
                        Status = OrderStatus.Placed
                    };

                    _orderRepository.AddOrder(order);
                    await _orderRepository.SaveChangesAsync();
                    foreach (var menuItem in menuItems)
                    {
                        var requestedItem = request.Items
                                                .First(x =>
                                                    x.MenuItemId == menuItem.Id);

                        var orderedItem = new OrderItem
                        {
                            OrderId = order.Id,
                            MenuItemId = menuItem.Id,
                            Quantity = requestedItem.Quantity,
                            Price = menuItem.Price * requestedItem.Quantity
                        };
                        _orderRepository.AddOrderItem(orderedItem);
                    }

                    await _orderRepository.SaveChangesAsync();
                    transaction.Commit();
                    return order.Id;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<OrderDetailsResponse> GetOrderDetailsAsync(
            long orderId, 
            long userId)
        {
            return await _orderRepository
                .GetOrderDetailsAsync(orderId, userId);
        }
    }
}
