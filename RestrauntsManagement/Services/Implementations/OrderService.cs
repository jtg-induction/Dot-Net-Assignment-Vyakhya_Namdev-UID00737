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

        public async Task<OrderResponse> PlaceOrderAsync(
            long userId,
            PlaceOrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    ErrorMessages.EmptyOrderItemsException);
            }

            var menuItemIds = request.Items
                .Select(x => x.MenuItemId)
                .ToHashSet();

            if (menuItemIds.Count != request.Items.Count)
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    ErrorMessages.DuplicateMenuItemException);
            }

            using (var transaction = _orderRepository.BeginTransaction(
                IsolationLevel.Serializable))
            {
                try
                {
                    var customer = await _userRepository.GetActiveUserByIdAsync(userId);
                    if (customer == null)
                    {
                        throw new ApiException(
                            HttpStatusCode.Unauthorized,
                            ErrorMessages.UserNotFound);
                    }

                    if (!customer.IsActive)
                    {
                        throw new ApiException(
                            HttpStatusCode.Forbidden,
                            ErrorMessages.AccessDenied);
                    }

                    var addressExists = await _orderRepository.DoesAddressBelongsToUserAsync(
                                                                userId,
                                                                request.DeliveryAddressId);

                    if (!addressExists)
                    {
                        throw new ApiException(
                            HttpStatusCode.BadRequest,
                            ErrorMessages.DeliveryAddressNotFound);
                    }

                    decimal totalAmount = 0;
                    long totalItems = 0;
                    var menuItems = await _orderRepository.GetMenuItemsAsync(
                                                        menuItemIds,
                                                        request.RestaurantId);

                    var foundMenuItemIds = menuItems
                        .Select(x => x.Id)
                        .ToHashSet();

                    var missingMenuItemIds = menuItemIds
                        .Except(foundMenuItemIds)
                        .ToList();

                    if (missingMenuItemIds.Any())
                    {
                        throw new ApiException(
                            HttpStatusCode.NotFound,
                            string.Format(
                                ErrorMessages.MenuItemNotFound,
                                string.Join(", ", missingMenuItemIds)));
                    }

                    var menuItemById = menuItems.ToDictionary(x => x.Id);
                    var orderItems = new List<OrderItem>();
                    foreach (var requestedItem in request.Items) {
                        var menuItem = menuItemById[requestedItem.MenuItemId]; 

                        if (menuItem.QuantityAvailable < requestedItem.Quantity)
                        {
                            throw new ApiException(
                                HttpStatusCode.BadRequest,
                                string.Format(
                                    ErrorMessages.InsufficientQuanityError,
                                    menuItem.Id,
                                    requestedItem.Quantity,
                                    menuItem.QuantityAvailable));
                        }

                        totalAmount += menuItem.Price * requestedItem.Quantity;
                        totalItems += requestedItem.Quantity;
                        menuItem.QuantityAvailable -= requestedItem.Quantity;

                        orderItems.Add(new OrderItem
                        {
                            MenuItemId = menuItem.Id,
                            Quantity = requestedItem.Quantity,
                            Price = menuItem.Price * requestedItem.Quantity
                        });

                    }

                    if (customer.Balance < totalAmount)
                    {
                        throw new ApiException(
                            HttpStatusCode.BadRequest,
                            ErrorMessages.InsufficientBalance);
                    }

                    customer.Balance -= totalAmount;
                    var order = new Order
                    {
                        CustomerId = userId,
                        RestaurantId = request.RestaurantId,
                        DeliveryAddressId = request.DeliveryAddressId,
                        TotalItems = totalItems,
                        TotalAmount = totalAmount,
                        Status = OrderStatus.Placed
                    };
                    _orderRepository.AddOrder(order);

                    foreach (var orderItem in orderItems)
                    {
                        orderItem.OrderId = order.Id;
                        _orderRepository.AddOrderItem(orderItem);
                    }

                    await _orderRepository.SaveChangesAsync();
                    transaction.Commit();
                    return new OrderResponse
                    {
                        OrderId = order.Id
                    };
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
