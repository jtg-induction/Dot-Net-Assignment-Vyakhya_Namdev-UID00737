using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Controllers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
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
                        Quantity = 2
                    }
                },
                DeliveryAddressId = 1,
                RestaurantId = 13
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
                .Setup(x => x.PlaceOrderAsync(It.IsAny<long>(), request))
                .ReturnsAsync(new OrderResponse
                {
                    OrderId = expectedOrderId
                });

            var result = await _controller.PlaceOrder(request);

            var createdResult = result
                .Should()
                .BeOfType<CreatedNegotiatedContentResult<ApiResponse<OrderResponse>>>()
                .Subject;

            createdResult.Content.Success.Should().BeTrue();
            createdResult.Content.Data.OrderId.Should().Be(expectedOrderId);

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
                .ReturnsAsync(new OrderResponse
                {
                    OrderId = 101
                });

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
                .ReturnsAsync(new OrderResponse
                {
                    OrderId = expectedOrderId
                });

            var result = await _controller.PlaceOrder(request);

            var createdResult = result
                .Should()
                .BeOfType<CreatedNegotiatedContentResult<ApiResponse<OrderResponse>>>()
                .Subject;

            createdResult.Content.Data.OrderId.Should().Be(expectedOrderId);
        }

        [TestMethod]
        [Description("Verify PlaceOrder should return success message")]
        public async Task PlaceOrder_ShouldReturnSuccessTrue()
        {
            PlaceOrderRequest request = InitializeRequest();

            _orderServiceMock
                .Setup(x => x.PlaceOrderAsync(It.IsAny<long>(), request))
                .ReturnsAsync(new OrderResponse
                {
                    OrderId = 101
                });

            var result = await _controller.PlaceOrder(request);

            var createdResult = result
                .Should()
                .BeOfType<CreatedNegotiatedContentResult<ApiResponse<OrderResponse>>>()
                .Subject;

            createdResult.Content.Success.Should().BeTrue();
        }

        [TestMethod]
        [Description("Check if PlaceOrder should call service only once")]
        public async Task PlaceOrder_ShouldCallServiceOnlyOnce()
        {
            PlaceOrderRequest request = InitializeRequest();

            _orderServiceMock
                .Setup(x => x.PlaceOrderAsync(It.IsAny<long>(), request))
                .ReturnsAsync(new OrderResponse
                {
                    OrderId = 101
                });

            await _controller.PlaceOrder(request);

            _orderServiceMock.Verify(
                x => x.PlaceOrderAsync(It.IsAny<long>(), request),
                Times.Once);
        }
    }
}
