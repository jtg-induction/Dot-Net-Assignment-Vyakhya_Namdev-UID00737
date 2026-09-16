using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;

namespace RestrauntsManagement.Tests.Services
{
    [TestClass]
    public class OrderServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IOrderRepository> _orderRepositoryMock;
        private Mock<ITransaction> _transactionMock;
        private OrderService _orderService;

        [TestInitialize]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _transactionMock = new Mock<ITransaction>();

            _orderRepositoryMock
                .Setup(x => x.BeginTransaction(IsolationLevel.Serializable))
                .Returns(_transactionMock.Object);

            _orderService = new OrderService(
                _userRepositoryMock.Object,
                _orderRepositoryMock.Object);
        }

        [TestMethod]
        public async Task PlaceOrderAsync_ShouldThrowBadRequest_WhenRequestIsNull()
        {
            Func<Task> act = async () =>
                await _orderService.PlaceOrderAsync(1, null);

            var exception = await act.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task PlaceOrderAsync_ShouldThrowBadRequest_WhenItemsAreEmpty()
        {
            var request = new PlaceOrderRequest
            {
                Items = new List<OrderItemRequest>(),
                DeliveryAddressId = 1
            };

            Func<Task> act = async () =>
                await _orderService.PlaceOrderAsync(1, request);

            var exception = await act.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task PlaceOrderAsync_ShouldThrowBadRequest_WhenQuantityIsZero()
        {
            var request = new PlaceOrderRequest
            {
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        MenuItemId = 32,
                        RestaurantId = 13,
                        Quantity = 0
                    }
                },
                DeliveryAddressId = 1
            };

            Func<Task> act = async () =>
                await _orderService.PlaceOrderAsync(1, request);

            var exception = await act.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task PlaceOrderAsync_ShouldThrowBadRequest_WhenDuplicateMenuItemExists()
        {
            var request = new PlaceOrderRequest
            {
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        MenuItemId = 32,
                        RestaurantId = 13,
                        Quantity = 1
                    },
                    new OrderItemRequest
                    {
                        MenuItemId = 32,
                        RestaurantId = 13,
                        Quantity = 2
                    }
                },
                DeliveryAddressId = 1
            };

            Func<Task> act = async () =>
                await _orderService.PlaceOrderAsync(1, request);

            var exception = await act.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task PlaceOrderAsync_ShouldThrowBadRequest_WhenItemsBelongToDifferentRestaurants()
        {
            var request = new PlaceOrderRequest
            {
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        MenuItemId = 32,
                        RestaurantId = 13,
                        Quantity = 1
                    },
                    new OrderItemRequest
                    {
                        MenuItemId = 37,
                        RestaurantId = 14,
                        Quantity = 1
                    }
                },
                DeliveryAddressId = 1
            };

            Func<Task> act = async () =>
                await _orderService.PlaceOrderAsync(1, request);

            var exception = await act.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task PlaceOrderAsync_ShouldThrowUnauthorized_WhenUserIsNotActive()
        {
            var request = CreateValidRequest();

            _userRepositoryMock
                .Setup(x => x.GetActiveUserAsync(1))
                .ReturnsAsync((User)null);

            Func<Task> act = async () =>
                await _orderService.PlaceOrderAsync(1, request);

            var exception = await act.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.Unauthorized);

            _transactionMock.Verify(
                x => x.Rollback(),
                Times.Once);
        }

        [TestMethod]
        public async Task PlaceOrderAsync_ShouldThrowBadRequest_WhenAddressDoesNotBelongToUser()
        {
            var request = CreateValidRequest();

            _userRepositoryMock
                .Setup(x => x.GetActiveUserAsync(1))
                .ReturnsAsync(new User
                {
                    Id = 1,
                    IsActive = true
                });

            _orderRepositoryMock
                .Setup(x => x.IsAddressBelongsToUserAsync(1, 1))
                .ReturnsAsync(false);

            Func<Task> act = async () =>
                await _orderService.PlaceOrderAsync(1, request);

            var exception = await act.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.BadRequest);

            _transactionMock.Verify(
                x => x.Rollback(),
                Times.Once);
        }

        [TestMethod]
        public async Task PlaceOrderAsync_ShouldThrowNotFound_WhenMenuItemDoesNotExist()
        {
            var request = CreateValidRequest();

            _userRepositoryMock
                .Setup(x => x.GetActiveUserAsync(1))
                .ReturnsAsync(new User
                {
                    Id = 1,
                    IsActive = true
                });

            _orderRepositoryMock
                .Setup(x => x.IsAddressBelongsToUserAsync(1, 1))
                .ReturnsAsync(true);

            _orderRepositoryMock
                .Setup(x => x.GetMenuItemAsync(32, 13))
                .ReturnsAsync((MenuItem)null);

            Func<Task> act = async () =>
                await _orderService.PlaceOrderAsync(1, request);

            var exception = await act.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.NotFound);

            _transactionMock.Verify(
                x => x.Rollback(),
                Times.Once);
        }

        [TestMethod]
        public async Task PlaceOrderAsync_ShouldThrowBadRequest_WhenQuantityIsInsufficient()
        {
            var request = CreateValidRequest();

            _userRepositoryMock
                .Setup(x => x.GetActiveUserAsync(1))
                .ReturnsAsync(new User
                {
                    Id = 1,
                    IsActive = true
                });

            _orderRepositoryMock
                .Setup(x => x.IsAddressBelongsToUserAsync(1, 1))
                .ReturnsAsync(true);

            _orderRepositoryMock
                .Setup(x => x.GetMenuItemAsync(32, 13))
                .ReturnsAsync(new MenuItem
                {
                    Id = 32,
                    RestaurantId = 13,
                    Price = 180,
                    QuantityAvailable = 1
                });

            Func<Task> act = async () =>
                await _orderService.PlaceOrderAsync(1, request);

            var exception = await act.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.BadRequest);

            _transactionMock.Verify(
                x => x.Rollback(),
                Times.Once);
        }

        [TestMethod]
        public async Task PlaceOrderAsync_ShouldPlaceOrderSuccessfully()
        {
            var request = new PlaceOrderRequest
            {
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        MenuItemId = 32,
                        RestaurantId = 13,
                        Quantity = 2
                    },
                    new OrderItemRequest
                    {
                        MenuItemId = 33,
                        RestaurantId = 13,
                        Quantity = 1
                    }
                },
                DeliveryAddressId = 1
            };

            _userRepositoryMock
                .Setup(x => x.GetActiveUserAsync(1))
                .ReturnsAsync(new User
                {
                    Id = 1,
                    IsActive = true
                });

            _orderRepositoryMock
                .Setup(x => x.IsAddressBelongsToUserAsync(1, 1))
                .ReturnsAsync(true);

            var menuItem1 = new MenuItem
            {
                Id = 32,
                RestaurantId = 13,
                Price = 180,
                QuantityAvailable = 10
            };

            var menuItem2 = new MenuItem
            {
                Id = 33,
                RestaurantId = 13,
                Price = 100,
                QuantityAvailable = 10
            };

            _orderRepositoryMock
                .Setup(x => x.GetMenuItemAsync(32, 13))
                .ReturnsAsync(menuItem1);

            _orderRepositoryMock
                .Setup(x => x.GetMenuItemAsync(33, 13))
                .ReturnsAsync(menuItem2);

            long generatedOrderId = 100;

            _orderRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Callback(() =>
                {
                    if (generatedOrderId == 100)
                    {
                        generatedOrderId = 101;
                    }
                })
                .Returns(Task.CompletedTask);

            Order capturedOrder = null;

            _orderRepositoryMock
                .Setup(x => x.AddOrder(It.IsAny<Order>()))
                .Callback<Order>(order =>
                {
                    order.Id = 101;
                    capturedOrder = order;
                });

            var capturedOrderItems = new List<OrderItem>();

            _orderRepositoryMock
                .Setup(x => x.AddOrderItem(It.IsAny<OrderItem>()))
                .Callback<OrderItem>(item =>
                {
                    capturedOrderItems.Add(item);
                });

            // Act
            var result = await _orderService.PlaceOrderAsync(1, request);

            // Assert
            result.Should().Be(101);

            capturedOrder.Should().NotBeNull();
            capturedOrder.CustomerId.Should().Be(1);
            capturedOrder.RestaurantId.Should().Be(13);
            capturedOrder.DeliveryAddressId.Should().Be(1);
            capturedOrder.TotalItems.Should().Be(3);
            capturedOrder.TotalAmount.Should().Be(460);
            capturedOrder.Status.Should().Be(OrderStatus.Placed);

            menuItem1.QuantityAvailable.Should().Be(8);
            menuItem2.QuantityAvailable.Should().Be(9);

            capturedOrderItems.Should().HaveCount(2);

            _orderRepositoryMock.Verify(
                x => x.AddOrder(It.IsAny<Order>()),
                Times.Once);

            _orderRepositoryMock.Verify(
                x => x.AddOrderItem(It.IsAny<OrderItem>()),
                Times.Exactly(2));

            _orderRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Exactly(2));

            _transactionMock.Verify(
                x => x.Commit(),
                Times.Once);

            _transactionMock.Verify(
                x => x.Rollback(),
                Times.Never);
        }

        [TestMethod]
        public async Task PlaceOrderAsync_ShouldRollbackTransaction_WhenRepositoryThrowsException()
        {
            var request = CreateValidRequest();

            _userRepositoryMock
                .Setup(x => x.GetActiveUserAsync(1))
                .ReturnsAsync(new User
                {
                    Id = 1,
                    IsActive = true
                });

            _orderRepositoryMock
                .Setup(x => x.IsAddressBelongsToUserAsync(1, 1))
                .ReturnsAsync(true);

            _orderRepositoryMock
                .Setup(x => x.GetMenuItemAsync(32, 13))
                .ReturnsAsync(new MenuItem
                {
                    Id = 32,
                    RestaurantId = 13,
                    Price = 180,
                    QuantityAvailable = 10
                });

            _orderRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new System.Exception("Database error"));

            Func<Task> act = async () =>
                await _orderService.PlaceOrderAsync(1, request);

            await act.Should().ThrowAsync<System.Exception>();

            _transactionMock.Verify(
                x => x.Rollback(),
                Times.Once);

            _transactionMock.Verify(
                x => x.Commit(),
                Times.Never);
        }

        private PlaceOrderRequest CreateValidRequest()
        {
            return new PlaceOrderRequest
            {
                Items = new List<OrderItemRequest>
                {
                    new OrderItemRequest
                    {
                        MenuItemId = 32,
                        RestaurantId = 13,
                        Quantity = 2
                    }
                },
                DeliveryAddressId = 1
            };
        }
    }
}
