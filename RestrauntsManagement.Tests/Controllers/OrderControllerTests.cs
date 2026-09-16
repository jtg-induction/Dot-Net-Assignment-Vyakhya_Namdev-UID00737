using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Controllers;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Services.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace RestrauntsManagement.Tests.Controllers
{
    [TestClass]
    public class OrderControllerTests
    {
        private Mock<IOrderService> _orderServiceMock;
        private OrderController _controller;

        [TestInitialize]
        public void SetUp()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _controller = new OrderController(_orderServiceMock.Object);

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(StringConstants.UserId, "1"));

            _controller.User = new ClaimsPrincipal(identity);
        }

        private PlaceOrderRequest InitializeRequest()
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
                    }
                },
                DeliveryAddressId = 1
            };

            return request;
        }

        [TestMethod]
        [Description("Check if PlaceOrder returns order_id when order placed successfully")]
        public async Task PlaceOrder_ShouldReturnOrderId_WhenOrderIsPlacedSuccessfully()
        {
            long userId = 1;
            long expectedOrderId = 101;
            PlaceOrderRequest request = InitializeRequest();

            _orderServiceMock
                .Setup(x => x.PlaceOrderAsync(userId, request))
                .ReturnsAsync(expectedOrderId);

            var result = await _controller.PlaceOrder(request);

            var okResult = result.Should()
                .BeOfType<OkNegotiatedContentResult<ApiResponse<long>>>()
                .Subject;

            okResult.Content.Success.Should().BeTrue();
            okResult.Content.Data.Should().Be(expectedOrderId);

            _orderServiceMock.Verify(
                x => x.PlaceOrderAsync(userId, request),
                Times.Once());
        }

        [TestMethod]
        [Description("Verifies PlaceOrder should pass correct user_id to service")]
        public async Task PlaceOrder_ShouldPassCorrectUserIdToService()
        {
            long userId = 1;
            PlaceOrderRequest request = InitializeRequest();

            _orderServiceMock
                .Setup(x => x.PlaceOrderAsync(It.IsAny<long>(), request))
                .ReturnsAsync(101);

            await _controller.PlaceOrder(request);

            _orderServiceMock.Verify(
                x => x.PlaceOrderAsync(userId, request),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies PlaceOrder should return correct order_id")]
        public async Task PlaceOrder_ShouldReturnCorrectOrderId()
        {
            long expectedOrderId = 50;
            PlaceOrderRequest request = InitializeRequest();

            _orderServiceMock
                .Setup(x => x.PlaceOrderAsync(It.IsAny<long>(), request))
                .ReturnsAsync(expectedOrderId);

            var result = await _controller.PlaceOrder(request);

            var okResult = result
                .Should()
                .BeOfType<OkNegotiatedContentResult<ApiResponse<long>>>()
                .Subject;

            okResult.Content.Data.Should().Be(expectedOrderId);
        }

        [TestMethod]
        [Description("Verify PlaceOrder should return success message")]
        public async Task PlaceOrder_ShouldReturnSuccessTrue()
        {
            PlaceOrderRequest request = InitializeRequest();

            _orderServiceMock
                .Setup(x => x.PlaceOrderAsync(It.IsAny<long>(), request))
                .ReturnsAsync(101);

            var result = await _controller.PlaceOrder(request);

            var okResult = result
                .Should()
                .BeOfType<OkNegotiatedContentResult<ApiResponse<long>>>()
                .Subject;

            okResult.Content.Success.Should().BeTrue();
        }

        [TestMethod]
        [Description("Check if PlaceOrder should call service only once")]
        public async Task PlaceOrder_ShouldCallServiceOnlyOnce()
        {
            PlaceOrderRequest request = InitializeRequest();

            _orderServiceMock
                .Setup(x => x.PlaceOrderAsync(It.IsAny<long>(), request))
                .ReturnsAsync(101);

            await _controller.PlaceOrder(request);

            _orderServiceMock.Verify(
                x => x.PlaceOrderAsync(It.IsAny<long>(), request),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that GetOrderDetails returns the order details successfully for an authenticated user")]
        public async Task GetOrderDetails_ShouldReturnOrderDetails_WhenOrderExists()
        {
            long userId = 1;
            long orderId = 100;

            var expectedOrder = new OrderDetailsResponse
            {
                OrderId = orderId,
                DeliveryAddress = 1,
                TotalAmount = 560,
                TotalItems = 3,
                RestaurantId = 10,
                Items = new List<OrderItemResponse>()
            };

            _orderServiceMock
                .Setup(x => x.GetOrderDetailsAsync(orderId, userId))
                .ReturnsAsync(expectedOrder);

            var result = await _controller.GetOrderDetails(orderId);

            var okResult = result
                .Should()
                .BeOfType<OkNegotiatedContentResult<ApiResponse<OrderDetailsResponse>>>()
                .Subject;

            okResult.Content.Success.Should().BeTrue();
            okResult.Content.Data.Should().BeEquivalentTo(expectedOrder);

            _orderServiceMock.Verify(
                x => x.GetOrderDetailsAsync(orderId, userId),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that GetOrderDetails throws NotFound when the requested order does not exist or does not belong to the authenticated user")]
        public async Task GetOrderDetails_ShouldThrowNotFound_WhenOrderDoesNotExist()
        {
            long userId = 1;
            long orderId = 999;

            _orderServiceMock
                .Setup(x => x.GetOrderDetailsAsync(orderId, userId))
                .ReturnsAsync((OrderDetailsResponse)null);

            Func<Task> act = async () =>
            {
                await _controller.GetOrderDetails(orderId);
            };

            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(ex =>
                    ex.StatusCode == HttpStatusCode.NotFound &&
                    ex.Message == ErrorMessages.OrderNotFound);

            _orderServiceMock.Verify(
                x => x.GetOrderDetailsAsync(orderId, userId),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that GetOrderDetails passes the authenticated user ID from claims to the service")]
        public async Task GetOrderDetails_ShouldPassCorrectUserIdToService()
        {
            long userId = 1;
            long orderId = 150;

            _orderServiceMock
                .Setup(x => x.GetOrderDetailsAsync(orderId, userId))
                .ReturnsAsync(new OrderDetailsResponse
                {
                    OrderId = orderId
                });

            await _controller.GetOrderDetails(orderId);

            _orderServiceMock.Verify(
                x => x.GetOrderDetailsAsync(orderId, userId),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that GetOrderDetails should return the correct order ID")]
        public async Task GetOrderDetails_ShouldReturnCorrectOrderId()
        {
            long orderId = 150;

            _orderServiceMock
                .Setup(x => x.GetOrderDetailsAsync(orderId, 1))
                .ReturnsAsync(new OrderDetailsResponse
                {
                    OrderId = orderId
                });

            var result = await _controller.GetOrderDetails(orderId);

            var okResult = result
                .Should()
                .BeOfType<OkNegotiatedContentResult<ApiResponse<OrderDetailsResponse>>>()
                .Subject;

            okResult.Content.Data.OrderId.Should().Be(orderId);
        }

        [TestMethod]
        [Description("Verifies that GetOrderDetails should call the service only once")]
        public async Task GetOrderDetails_ShouldCallServiceOnlyOnce()
        {
            long orderId = 100;

            _orderServiceMock
                .Setup(x => x.GetOrderDetailsAsync(orderId, 1))
                .ReturnsAsync(new OrderDetailsResponse
                {
                    OrderId = orderId
                });

            await _controller.GetOrderDetails(orderId);

            _orderServiceMock.Verify(
                x => x.GetOrderDetailsAsync(orderId, 1),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies CancelOrder returns a successful response with cancelled status when the order is cancelled successfully")]
        public async Task CancelOrder_ShouldReturnOk_WhenOrderIsCancelled()
        {
            long orderId = 10;
            long userId = 1;

            var serviceResponse = new CancelOrderResponse
            {
                OrderStatus = OrderStatus.Cancelled
            };

            _orderServiceMock
                .Setup(service => service.CancelOrderAsync(orderId, userId))
                .ReturnsAsync(serviceResponse);

            var result = await _controller.CancelOrder(orderId);

            var okResult = result
                .Should()
                .BeOfType<OkNegotiatedContentResult<ApiResponse<CancelOrderResponse>>>()
                .Subject;

            okResult.Content.Success.Should().BeTrue();
            okResult.Content.Data.Should().NotBeNull();
            okResult.Content.Data.OrderStatus.Should().Be(OrderStatus.Cancelled);
            okResult.Content.Message.Should().Be(SuccessMessages.OrderCancelled);

            _orderServiceMock.Verify(
                service => service.CancelOrderAsync(orderId, userId),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies CancelOrder passes the authenticated user ID from the UserId claim and the correct order ID to the service")]
        public async Task CancelOrder_ShouldPassCorrectIdsToService()
        {
            long orderId = 25;
            long userId = 1;

            _orderServiceMock
                .Setup(service => service.CancelOrderAsync(orderId, userId))
                .ReturnsAsync(new CancelOrderResponse
                {
                    OrderStatus = OrderStatus.Cancelled
                });

            await _controller.CancelOrder(orderId);

            _orderServiceMock.Verify(
                service => service.CancelOrderAsync(
                    It.Is<long>(id => id == orderId),
                    It.Is<long>(id => id == userId)),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies CancelOrder gives the NotFound ApiException when the requested order does not exist")]
        public async Task CancelOrder_ShouldThrowNotFound_WhenOrderDoesNotExist()
        {
            long orderId = 999;
            long userId = 1;

            _orderServiceMock
                .Setup(service => service.CancelOrderAsync(orderId, userId))
                .ThrowsAsync(
                    new ApiException(
                        HttpStatusCode.NotFound,
                        ErrorMessages.OrderNotFound));

            Func<Task> act = async () =>
                await _controller.CancelOrder(orderId);

            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(exception =>
                    exception.StatusCode == HttpStatusCode.NotFound &&
                    exception.Message == ErrorMessages.OrderNotFound);

            _orderServiceMock.Verify(
                service => service.CancelOrderAsync(orderId, userId),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies CancelOrder gives the BadRequest Exception when the order cannot be cancelled")]
        public async Task CancelOrder_ShouldThrowBadRequest_WhenOrderCannotBeCancelled()
        {
            long orderId = 10;
            long userId = 1;

            _orderServiceMock
                .Setup(service => service.CancelOrderAsync(orderId, userId))
                .ThrowsAsync(
                    new ApiException(
                        HttpStatusCode.BadRequest,
                        ErrorMessages.OrderCannotBeCancelled));

            Func<Task> act = async () =>
                await _controller.CancelOrder(orderId);

            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(exception =>
                    exception.StatusCode == HttpStatusCode.BadRequest &&
                    exception.Message == ErrorMessages.OrderCannotBeCancelled);

            _orderServiceMock.Verify(
                service => service.CancelOrderAsync(orderId, userId),
                Times.Once);
        }
    }
}
