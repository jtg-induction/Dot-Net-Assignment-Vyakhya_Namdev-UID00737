using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Controllers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Services.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace DotNetRestaurantManagement.Tests.Controllers
{
    [TestClass]
    public class OwnerOrderControllerTests
    {
        private Mock<IOwnerOrderService> _orderServiceMock;
        private OwnerOrderController _controller;

        [TestInitialize]
        public void Setup()
        {
            _orderServiceMock = new Mock<IOwnerOrderService>();
            _controller = new OwnerOrderController(_orderServiceMock.Object);
        }

        private void SetUserClaims(long userId)
        {
            var claims = new[]
            {
                new Claim("userId", userId.ToString()),
                new Claim("refreshTokenId", "10"),
                new Claim("role", "Customer")
            };

            _controller.User =
                new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "TestAuth"));
        }

        [TestMethod]
        [Description("Returns orders successfully for the authenticated owner")]
        public async Task GetOrders_ShouldReturnOk_WhenRequestIsValid()
        {
            long userId = 1;
            var request = new DashboardOrderRequest();
            var response =
                new PaginationResult<DashboardOrderResponse>();

            SetUserClaims(userId);

            _orderServiceMock
                .Setup(x => x.GetOrdersAsync(userId, request))
                .ReturnsAsync(response);

            var result = await _controller.GetOrders(request);

            result.Should()
                .BeOfType<OkNegotiatedContentResult<ApiResponse<object>>>();

            var okResult =
                (OkNegotiatedContentResult<ApiResponse<object>>)result;

            okResult.Content.Success.Should().BeTrue();
            okResult.Content.Data.Should().Be(response);
            okResult.Content.Message.Should()
                .Be(SuccessMessages.OrdersFetched);

            _orderServiceMock.Verify(
                x => x.GetOrdersAsync(userId, request),
                Times.Once);
        }

        [TestMethod]
        [Description("Creates a default request when the request is null")]
        public async Task GetOrders_ShouldCreateDefaultRequest_WhenRequestIsNull()
        {
            long userId = 1;
            DashboardOrderRequest capturedRequest = null;
            var response =
                new PaginationResult<DashboardOrderResponse>();

            SetUserClaims(userId);
            _orderServiceMock
                .Setup(x => x.GetOrdersAsync(
                    userId,
                    It.IsAny<DashboardOrderRequest>()))
                .Callback<long, DashboardOrderRequest>(
                    (id, request) =>
                    {
                        capturedRequest = request;
                    })
                .ReturnsAsync(response);

            var result = await _controller.GetOrders(null);

            result.Should()
                .BeOfType<OkNegotiatedContentResult<ApiResponse<object>>>();

            capturedRequest.Should().NotBeNull();

            _orderServiceMock.Verify(
                x => x.GetOrdersAsync(
                    userId,
                    It.IsAny<DashboardOrderRequest>()),
                Times.Once);
        }

        [TestMethod]
        [Description("Passes the authenticated user ID to the service")]
        public async Task GetOrders_ShouldPassCorrectUserId_ToService()
        {
            long userId = 25;
            var request = new DashboardOrderRequest();
            var response =
                new PaginationResult<DashboardOrderResponse>();

            SetUserClaims(userId);

            _orderServiceMock
                .Setup(x => x.GetOrdersAsync(
                    It.IsAny<long>(),
                    It.IsAny<DashboardOrderRequest>()))
                .ReturnsAsync(response);

            await _controller.GetOrders(request);

            _orderServiceMock.Verify(
                x => x.GetOrdersAsync(
                    userId,
                    request),
                Times.Once);
        }

        [TestMethod]
        [Description("Returns the response received from the order service")]
        public async Task GetOrders_ShouldReturnServiceResponse()
        {
            long userId = 5;
            var request = new DashboardOrderRequest();
            var serviceResponse =
                new PaginationResult<DashboardOrderResponse>();

            SetUserClaims(userId);

            _orderServiceMock
                .Setup(x => x.GetOrdersAsync(userId, request))
                .ReturnsAsync(serviceResponse);

            var result = await _controller.GetOrders(request);

            var okResult =
                result
                    .Should()
                    .BeOfType<OkNegotiatedContentResult<ApiResponse<object>>>()
                    .Subject;

            okResult.Content.Data
                .Should()
                .BeSameAs(serviceResponse);
        }

        [TestMethod]
        [Description("Updates order status successfully for the owner")]
        public async Task UpdateOrderStatus_ShouldReturnOk_WhenRequestIsValid()
        {
            long ownerId = 10;
            long restaurantId = 20;
            long orderId = 30;

            var request = new UpdateOrderStatus
            {
                Status = OrderStatus.Accepted
            };

            var response = new UpdateOrderStatus
            {
                Status = OrderStatus.Accepted
            };

            SetUserClaims(ownerId);

            _orderServiceMock
                .Setup(x => x.UpdateOrderStatusAsync(
                    ownerId,
                    restaurantId,
                    orderId,
                    request))
                .ReturnsAsync(response);

            var result = await _controller.UpdateOrderStatus(
                restaurantId,
                orderId,
                request);

            result.Should()
                .BeOfType<OkNegotiatedContentResult<ApiResponse<object>>>();

            var okResult =
                (OkNegotiatedContentResult<ApiResponse<object>>)result;

            okResult.Content.Success.Should().BeTrue();
            okResult.Content.Data.Should().Be(response);
            okResult.Content.Message.Should()
                .Be(SuccessMessages.OrderStatusUpdated);

            _orderServiceMock.Verify(
                x => x.UpdateOrderStatusAsync(
                    ownerId,
                    restaurantId,
                    orderId,
                    request),
                Times.Once);
        }

        [TestMethod]
        [Description("Passes owner, restaurant and order IDs to the service")]
        public async Task UpdateOrderStatus_ShouldPassCorrectParameters_ToService()
        {
            long ownerId = 15;
            long restaurantId = 25;
            long orderId = 35;

            var request = new UpdateOrderStatus
            {
                Status = OrderStatus.Dispatched
            };

            SetUserClaims(ownerId);

            _orderServiceMock
                .Setup(x => x.UpdateOrderStatusAsync(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<UpdateOrderStatus>()))
                .ReturnsAsync(new UpdateOrderStatus
                {
                    Status = OrderStatus.Dispatched
                });

            await _controller.UpdateOrderStatus(
                restaurantId,
                orderId,
                request);

            _orderServiceMock.Verify(
                x => x.UpdateOrderStatusAsync(
                    ownerId,
                    restaurantId,
                    orderId,
                    request),
                Times.Once);
        }
    }
}
