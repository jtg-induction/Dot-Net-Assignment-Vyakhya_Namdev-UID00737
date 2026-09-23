using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Interfaces;
using DotNetRestaurantManagement.Models.DTO;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Implementations
{
    public class OwnerOrderService : IOwnerOrderService
    {
        private readonly IOwnerOrderRepository _orderRepository;

        public OwnerOrderService(
            IOwnerOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<PaginationResult<DashboardOrderResponse>> GetOrdersAsync(
                long ownerId,
                DashboardOrderRequest request)
        {
            IQueryable<Order> query = _orderRepository.GetOrders(ownerId);

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.Trim();
                query = query.Where(order =>
                    order.Customer.Name.Contains(search) ||
                    order.OrderedItems.Any(item =>
                        item.MenuItem.Name.Contains(search)) ||
                    order.DeliveryAddress.HouseNumber.Contains(search) ||
                    order.DeliveryAddress.StreetAddress.Contains(search) ||
                    order.DeliveryAddress.City.Contains(search) ||
                    order.DeliveryAddress.State.Contains(search) ||
                    order.DeliveryAddress.PinCode.Contains(search) ||
                    order.DeliveryAddress.Country.Contains(search)
                );
            }

            // Filters
            if (request.RestaurantId.HasValue)
            {
                query = query.Where(order =>
                    order.RestaurantId == request.RestaurantId.Value);
            }

            if (request.CustomerId.HasValue)
            {
                query = query.Where(order =>
                    order.CustomerId == request.CustomerId.Value);
            }

            if (request.TotalItems > 0)
            {
                query = query.Where(order =>
                    order.TotalItems == request.TotalItems);
            }

            if (request.MinTotalItems.HasValue)
            {
                query = query.Where(order =>
                    order.TotalItems >= request.MinTotalItems.Value);
            }

            if (request.MaxTotalItems.HasValue)
            {
                query = query.Where(order =>
                    order.TotalItems <= request.MaxTotalItems.Value);
            }

            if (request.TotalAmount > 0)
            {
                query = query.Where(order =>
                    order.TotalAmount == request.TotalAmount);
            }

            if (request.MinTotalAmount.HasValue)
            {
                query = query.Where(order =>
                    order.TotalAmount >= request.MinTotalAmount.Value);
            }

            if (request.MaxTotalAmount.HasValue)
            {
                query = query.Where(order =>
                    order.TotalAmount <= request.MaxTotalAmount.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(order =>
                    order.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(order =>
                    order.CreatedAt <= request.ToDate);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (Enum.TryParse<OrderStatus>(
                    request.Status,
                    true,
                    out var status))
                {
                    query = query.Where(order =>
                        order.Status == status);
                }
            }

            // Sorting
            bool descending = string.Equals(
                request.SortOrder,
                StringConstants.DescendingOrder,
                StringComparison.OrdinalIgnoreCase);

            switch (request.SortBy)
            {
                case "totalAmount":
                    query = descending
                        ? query.OrderByDescending(order => order.TotalAmount)
                        : query.OrderBy(order => order.TotalAmount);
                    break;

                case "createdDate":
                    query = descending
                        ? query.OrderByDescending(order => order.CreatedAt)
                        : query.OrderBy(order => order.CreatedAt);
                    break;

                case "updatedDate":
                    query = descending
                        ? query.OrderByDescending(order => order.UpdatedAt)
                        : query.OrderBy(order => order.UpdatedAt);
                    break;

                default:
                    query = query.OrderByDescending(order => order.CreatedAt);
                    break;
            }

            var result = query.Select(order => new DashboardOrderResponse
            {
                OrderId = order.Id,
                RestaurantId = order.RestaurantId,
                RestaurantName = order.Restaurant.Name,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer.Name,
                TotalItems = order.TotalItems,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                DeliveryAddress = new RestaurantAddressDto
                {
                    Id = order.DeliveryAddress.Id,
                    HouseNumber = order.DeliveryAddress.HouseNumber,
                    StreetAddress = order.DeliveryAddress.StreetAddress,
                    City = order.DeliveryAddress.City,
                    State = order.DeliveryAddress.State,
                    PinCode = order.DeliveryAddress.PinCode,
                    Country = order.DeliveryAddress.Country
                }
            });

            return await PaginationHelper.CreateAsync(
                result,
                request);
        }

        private void ValidateStatus(
            OrderStatus currentStatus,
            OrderStatus newStatus)
        {
            if (currentStatus == newStatus)
            {
                throw new ApiException(
                    HttpStatusCode.Conflict,
                    ErrorMessages.SameStatusError);
            }

            bool isValid;
            switch (currentStatus)
            {
                case OrderStatus.Placed:
                    isValid = newStatus == OrderStatus.Accepted ||
                              newStatus == OrderStatus.Rejected;
                    break;

                case OrderStatus.Accepted:
                    isValid = newStatus == OrderStatus.Dispatched;
                    break;

                case OrderStatus.Dispatched:
                    isValid = newStatus == OrderStatus.Delivered;
                    break;

                case OrderStatus.Delivered:
                case OrderStatus.Rejected:
                case OrderStatus.Cancelled:
                    isValid = false;
                    break;

                default:
                    isValid = false;
                    break;
            }

            if (!isValid)
            {
                throw new ApiException(
                    HttpStatusCode.Conflict,
                    ErrorMessages.OrderStatusCannotChange);
            }
        }
        public async Task<UpdateOrderStatus> UpdateOrderStatusAsync(
            long ownerId,
            long restaurantId,
            long orderId,
            UpdateOrderStatus request)
        {
            var order = await _orderRepository
                .GetOrderForStatusUpdateAsync(ownerId, restaurantId, orderId);

            if (order == null)
            {
                throw new ApiException(
                    HttpStatusCode.NotFound,
                    ErrorMessages.OrderNotFound);
            }

            ValidateStatus(order.Status, request.Status);
            order.Status = request.Status;
            await _orderRepository.SaveChangesAsync();
            return new UpdateOrderStatus
            {
                Status = order.Status
            };
        }

        Task<PaginationResult<DashboardOrderResponse>> IOwnerOrderService.GetOrdersAsync(long ownerId, DashboardOrderRequest request)
        {
            throw new System.NotImplementedException();
        }

        Task<UpdateOrderStatus> IOwnerOrderService.UpdateOrderStatusAsync(long ownerId, long restaurantId, long orderId, UpdateOrderStatus request)
        {
            throw new System.NotImplementedException();
        }
    }
}
