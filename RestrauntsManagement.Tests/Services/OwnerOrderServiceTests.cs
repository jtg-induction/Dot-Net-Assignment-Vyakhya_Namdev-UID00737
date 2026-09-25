using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Implementations;
using DotNetRestaurantManagement.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Tests.Services
{
    [TestClass]
    public class OwnerOrderServiceTests
    {
        private Mock<IOwnerOrderRepository> _orderRepositoryMock;
        private OwnerOrderService _service;

        [TestInitialize]
        public void Setup()
        {
            _orderRepositoryMock = new Mock<IOwnerOrderRepository>();
            _service = new OwnerOrderService(
                _orderRepositoryMock.Object);
        }

        private Order CreateOrder(
            int id,
            long ownerId,
            long restaurantId,
            long customerId,
            string customerName,
            string menuItemName,
            OrderStatus status = OrderStatus.Placed)
        {
            var owner = new User
            {
                Id = ownerId,
                Name = "Owner",
                Email = $"owner{ownerId}@test.com",
                Password = "HASH",
                PhoneNumber = $"90000000{id:D2}",
                Role = UserRole.Owner
            };

            var restaurant = new Restaurant
            {
                Id = restaurantId,
                OwnerId = ownerId,
                Owner = owner,
                Name = $"Restaurant {restaurantId}",
                Email = $"restaurant{restaurantId}@test.com"
            };

            var customer = new User
            {
                Id = customerId,
                Name = customerName,
                Email = $"customer{customerId}@test.com",
                Password = "HASH",
                PhoneNumber = $"80000000{id:D2}",
                Role = UserRole.Customer
            };

            var menuItem = new MenuItem
            {
                Id = id,
                Name = menuItemName
            };

            var order = new Order
            {
                Id = id,
                RestaurantId = restaurantId,
                Restaurant = restaurant,
                CustomerId = customerId,
                Customer = customer,
                TotalItems = 3,
                TotalAmount = 500,
                Status = status,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            order.OrderedItems.Add(new OrderItem
            {
                Id = id,
                OrderId = id,
                MenuItemId = id,
                MenuItem = menuItem,
                Quantity = 2,
                Price = 250
            });

            order.DeliveryAddress = new Address
            {
                Id = id,
                HouseNumber = "101",
                StreetAddress = "Test Street",
                City = "Delhi",
                State = "Delhi",
                PinCode = "110001",
                Country = "India"
            };

            return order;
        }

        private List<Order> CreateOrders()
        {
            return new List<Order>
            {
                CreateOrder(
                    1,
                    10,
                    100,
                    20,
                    "John Doe",
                    "Pizza",
                    OrderStatus.Placed),

                CreateOrder(
                    2,
                    10,
                    101,
                    21,
                    "Jane Doe",
                    "Burger",
                    OrderStatus.Accepted),

                CreateOrder(
                    3,
                    10,
                    102,
                    22,
                    "Robert",
                    "Pasta",
                    OrderStatus.Delivered)
            };
        }

        [TestMethod]
        [Description("Should return paginated orders for owner")]
        public async Task GetOrdersAsync_WhenRequestIsValid_ReturnsOrders()
        {
            var orders = CreateOrders();

            _orderRepositoryMock
                .Setup(x => x.GetOrders(10))
                .Returns(new TestAsyncEnumerable<Order>(orders));

            var request = new DashboardOrderRequest
            {
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetOrdersAsync(10, request);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(3);
            result.TotalCount.Should().Be(3);

            _orderRepositoryMock.Verify(
                x => x.GetOrders(10),
                Times.Once);
        }

        [TestMethod]
        [Description("Should filter orders by customer name")]
        public async Task GetOrdersAsync_WhenCustomerNameProvided_FiltersOrders()
        {
            var orders = CreateOrders();

            _orderRepositoryMock
                .Setup(x => x.GetOrders(10))
                .Returns(new TestAsyncEnumerable<Order>(orders));

            var request = new DashboardOrderRequest
            {
                CustomerName = "John",
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetOrdersAsync(10, request);

            result.Items.Should().HaveCount(1);
            result.Items.First().CustomerName.Should().Be("John Doe");
        }

        [TestMethod]
        [Description("Should filter orders by menu item name")]
        public async Task GetOrdersAsync_WhenMenuItemNameProvided_FiltersOrders()
        {
            var orders = CreateOrders();

            _orderRepositoryMock
                .Setup(x => x.GetOrders(10))
                .Returns(new TestAsyncEnumerable<Order>(orders));

            var request = new DashboardOrderRequest
            {
                MenuItemName = "Burger",
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetOrdersAsync(10, request);

            result.Items.Should().HaveCount(1);
            result.Items.First().OrderId.Should().Be(2);
        }

        [TestMethod]
        [Description("Should filter orders by delivery address")]
        public async Task GetOrdersAsync_WhenAddressSearchProvided_FiltersOrders()
        {
            var orders = CreateOrders();

            _orderRepositoryMock
                .Setup(x => x.GetOrders(10))
                .Returns(new TestAsyncEnumerable<Order>(orders));

            var request = new DashboardOrderRequest
            {
                AddressSearch = "Delhi",
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetOrdersAsync(10, request);

            result.Items.Should().HaveCount(3);
        }

        [TestMethod]
        [Description("Should apply order filters")]
        public async Task GetOrdersAsync_WhenFiltersProvided_FiltersOrders()
        {
            var orders = CreateOrders();

            _orderRepositoryMock
                .Setup(x => x.GetOrders(10))
                .Returns(new TestAsyncEnumerable<Order>(orders));

            var request = new DashboardOrderRequest
            {
                RestaurantId = 101,
                CustomerId = 21,
                TotalItems = 3,
                MinTotalItems = 2,
                MaxTotalItems = 5,
                TotalAmount = 500,
                MinTotalAmount = 400,
                MaxTotalAmount = 600,
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetOrdersAsync(10, request);

            result.Items.Should().HaveCount(1);
            result.Items.First().OrderId.Should().Be(2);
        }

        [TestMethod]
        [Description("Should filter orders by date range")]
        public async Task GetOrdersAsync_WhenDateRangeProvided_FiltersOrders()
        {
            var orders = CreateOrders();
            _orderRepositoryMock
                .Setup(x => x.GetOrders(10))
                .Returns(new TestAsyncEnumerable<Order>(orders));

            var request = new DashboardOrderRequest
            {
                FromDate = DateTime.UtcNow.AddDays(-3),
                ToDate = DateTime.UtcNow,
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetOrdersAsync(10, request);

            result.Items.Should().HaveCount(3);
        }

        [TestMethod]
        [Description("Should filter orders by status")]
        public async Task GetOrdersAsync_WhenStatusProvided_FiltersOrders()
        {
            var orders = CreateOrders();

            _orderRepositoryMock
                .Setup(x => x.GetOrders(10))
                .Returns(new TestAsyncEnumerable<Order>(orders));

            var request = new DashboardOrderRequest
            {
                Status = "Accepted",
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetOrdersAsync(10, request);

            result.Items.Should().HaveCount(1);
            result.Items.First().Status.Should().Be("Accepted");
        }

        [TestMethod]
        [Description("Should apply total amount ascending sorting")]
        public async Task GetOrdersAsync_WhenSortByTotalAmountAscending_SortsOrders()
        {
            var orders = CreateOrders();

            orders[0].TotalAmount = 300;
            orders[1].TotalAmount = 100;
            orders[2].TotalAmount = 200;

            _orderRepositoryMock
                .Setup(x => x.GetOrders(10))
                .Returns(new TestAsyncEnumerable<Order>(orders));

            var request = new DashboardOrderRequest
            {
                SortBy = "totalamount",
                SortOrder = "asc",
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetOrdersAsync(10, request);

            result.Items
                .Select(x => x.TotalAmount)
                .Should()
                .ContainInOrder(100, 200, 300);
        }

        [TestMethod]
        [Description("Should apply total amount descending sorting")]
        public async Task GetOrdersAsync_WhenSortByTotalAmountDescending_SortsOrders()
        {
            var orders = CreateOrders();

            orders[0].TotalAmount = 300;
            orders[1].TotalAmount = 100;
            orders[2].TotalAmount = 200;

            _orderRepositoryMock
                .Setup(x => x.GetOrders(10))
                .Returns(new TestAsyncEnumerable<Order>(orders));

            var request = new DashboardOrderRequest
            {
                SortBy = "totalamount",
                SortOrder = "desc",
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetOrdersAsync(10, request);

            result.Items
                .Select(x => x.TotalAmount)
                .Should()
                .ContainInOrder(300, 200, 100);
        }

        [TestMethod]
        [Description("Should apply created date ascending sorting")]
        public async Task GetOrdersAsync_WhenSortByCreatedDate_SortsOrders()
        {
            var orders = CreateOrders();

            orders[0].CreatedAt = DateTime.UtcNow.AddDays(-5);
            orders[1].CreatedAt = DateTime.UtcNow.AddDays(-2);
            orders[2].CreatedAt = DateTime.UtcNow.AddDays(-3);

            _orderRepositoryMock
                .Setup(x => x.GetOrders(10))
                .Returns(new TestAsyncEnumerable<Order>(orders));

            var request = new DashboardOrderRequest
            {
                SortBy = "createddate",
                SortOrder = "asc",
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetOrdersAsync(10, request);

            result.Items
                .Select(x => x.OrderId)
                .Should()
                .ContainInOrder(1, 3, 2);
        }

        [TestMethod]
        [Description("Should apply updated date ascending sorting")]
        public async Task GetOrdersAsync_WhenSortByUpdatedDate_SortsOrders()
        {
            var orders = CreateOrders();

            orders[0].UpdatedAt = DateTime.UtcNow.AddDays(-5);
            orders[1].UpdatedAt = DateTime.UtcNow.AddDays(-2);
            orders[2].UpdatedAt = DateTime.UtcNow.AddDays(-3);

            _orderRepositoryMock
                .Setup(x => x.GetOrders(10))
                .Returns(new TestAsyncEnumerable<Order>(orders));

            var request = new DashboardOrderRequest
            {
                SortBy = "updateddate",
                SortOrder = "asc",
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetOrdersAsync(10, request);

            result.Items
                .Select(x => x.OrderId)
                .Should()
                .ContainInOrder(1, 3, 2);
        }

        [TestMethod]
        [Description("Should use created date descending as default sorting")]
        public async Task GetOrdersAsync_WhenSortIsNotProvided_UsesDefaultSorting()
        {
            var orders = CreateOrders();

            orders[0].CreatedAt = DateTime.UtcNow.AddDays(-5);
            orders[1].CreatedAt = DateTime.UtcNow.AddDays(-2);
            orders[2].CreatedAt = DateTime.UtcNow.AddDays(-3);

            _orderRepositoryMock
                .Setup(x => x.GetOrders(10))
                .Returns(new TestAsyncEnumerable<Order>(orders));

            var request = new DashboardOrderRequest
            {
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetOrdersAsync(10, request);

            result.Items
                .Select(x => x.OrderId)
                .Should()
                .ContainInOrder(2, 3, 1);
        }

        [TestMethod]
        [Description("Should update order status when transition is valid")]
        public async Task UpdateOrderStatusAsync_WhenStatusTransitionIsValid_UpdatesStatus()
        {
            var order = new Order
            {
                Id = 1,
                RestaurantId = 100,
                Status = OrderStatus.Placed
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForStatusUpdateAsync(10, 100, 1))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var request = new UpdateOrderStatus
            {
                Status = OrderStatus.Accepted
            };

            var result = await _service.UpdateOrderStatusAsync(
                10,
                100,
                1,
                request);

            result.Status.Should().Be(OrderStatus.Accepted);
            order.Status.Should().Be(OrderStatus.Accepted);

            _orderRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [TestMethod]
        [Description("Should throw conflict when same status is requested")]
        public async Task UpdateOrderStatusAsync_WhenSameStatusIsRequested_ThrowsConflict()
        {
            var order = new Order
            {
                Id = 1,
                RestaurantId = 100,
                Status = OrderStatus.Placed
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForStatusUpdateAsync(10, 100, 1))
                .ReturnsAsync(order);

            var request = new UpdateOrderStatus
            {
                Status = OrderStatus.Placed
            };

            Func<Task> action = async () =>
                await _service.UpdateOrderStatusAsync(
                    10,
                    100,
                    1,
                    request);

            await action.Should().ThrowAsync<ApiException>();
        }

        [TestMethod]
        [Description("Should throw conflict when invalid status transition is requested")]
        public async Task UpdateOrderStatusAsync_WhenInvalidStatusTransition_ThrowsConflict()
        {
            var order = new Order
            {
                Id = 1,
                RestaurantId = 100,
                Status = OrderStatus.Placed
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForStatusUpdateAsync(10, 100, 1))
                .ReturnsAsync(order);

            var request = new UpdateOrderStatus
            {
                Status = OrderStatus.Delivered
            };

            Func<Task> action = async () =>
                await _service.UpdateOrderStatusAsync(
                    10,
                    100,
                    1,
                    request);

            await action.Should().ThrowAsync<ApiException>();
        }

        [TestMethod]
        [Description("Should allow accepted to dispatched transition")]
        public async Task UpdateOrderStatusAsync_WhenAccepted_ChangesToDispatched()
        {
            var order = new Order
            {
                Id = 1,
                RestaurantId = 100,
                Status = OrderStatus.Accepted
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForStatusUpdateAsync(10, 100, 1))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var request = new UpdateOrderStatus
            {
                Status = OrderStatus.Dispatched
            };

            var result = await _service.UpdateOrderStatusAsync(
                10,
                100,
                1,
                request);

            result.Status.Should().Be(OrderStatus.Dispatched);
        }

        [TestMethod]
        [Description("Should allow dispatched to delivered transition")]
        public async Task UpdateOrderStatusAsync_WhenDispatched_ChangesToDelivered()
        {
            var order = new Order
            {
                Id = 1,
                RestaurantId = 100,
                Status = OrderStatus.Dispatched
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForStatusUpdateAsync(10, 100, 1))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var request = new UpdateOrderStatus
            {
                Status = OrderStatus.Delivered
            };

            var result = await _service.UpdateOrderStatusAsync(
                10,
                100,
                1,
                request);

            result.Status.Should().Be(OrderStatus.Delivered);
        }

        [TestMethod]
        [Description("Should allow placed to rejected transition")]
        public async Task UpdateOrderStatusAsync_WhenPlaced_ChangesToRejected()
        {
            var order = new Order
            {
                Id = 1,
                RestaurantId = 100,
                Status = OrderStatus.Placed
            };

            _orderRepositoryMock
                .Setup(x => x.GetOrderForStatusUpdateAsync(10, 100, 1))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var request = new UpdateOrderStatus
            {
                Status = OrderStatus.Rejected
            };

            var result = await _service.UpdateOrderStatusAsync(
                10,
                100,
                1,
                request);

            result.Status.Should().Be(OrderStatus.Rejected);
        }
    }
}
