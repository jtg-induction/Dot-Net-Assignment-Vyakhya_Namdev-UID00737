using DotNetRestaurantManagement.Controllers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace DotNetRestaurantManagement.Tests.Controllers
{
    [TestClass]
    public class RestaurantControllerTests
    {
        private Mock<IRestaurantService> _serviceMock;
        private RestaurantController _controller;

        [TestInitialize]
        public void Setup()
        {
            _serviceMock = new Mock<IRestaurantService>();
            _controller = new RestaurantController(_serviceMock.Object);
            _controller.Request = new System.Net.Http.HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        [TestMethod]
        public async Task GetRestaurants_ShouldReturnOk_WhenServiceSucceeds()
        {
            var expectedResult = new PaginationResult<RestaurantDto>
                {
                    Items = new List<RestaurantDto>
                    {
                        new RestaurantDto
                        {
                            Id = 1,
                            Name = "Spice Garden",
                            City = "Pune",
                            State = "Maharashtra",
                            PinCode = "411001",
                            Country = "India",
                            Cuisine = "Indian"
                        }
                    },
                    Page = 1,
                    PageSize = 10,
                    TotalCount = 1,
                    TotalPages = 1,
                    HasPreviousPage = false,
                    HasNextPage = false
                };

            _serviceMock
                .Setup(x =>
                    x.GetPaginationResultAsync(
                        It.IsAny<PaginationRequest>()))
                .ReturnsAsync(expectedResult);

            var request = new PaginationRequest
                {
                    Page = 1,
                    PageSize = 10
                };

            var result = await _controller.GetRestaurants(request);
            result.Should()
                .BeOfType<
                    OkNegotiatedContentResult<
                        PaginationResult<RestaurantDto>>>();

            var okResult = result as OkNegotiatedContentResult<PaginationResult<RestaurantDto>>;
            okResult.Content
                .Should()
                .BeSameAs(expectedResult);

            _serviceMock.Verify(
                x => x.GetPaginationResultAsync(
                    It.Is<PaginationRequest>(r =>
                        r.Page == 1 &&
                        r.PageSize == 10)),
                Times.Once);
        }

        [TestMethod]
        public async Task GetRestaurants_ShouldUseDefaultPagination_WhenRequestIsNull()
        {
            var expectedResult =
                new PaginationResult<RestaurantDto>
                {
                    Items = new List<RestaurantDto>(),
                    Page = 1,
                    PageSize = 10,
                    TotalCount = 0,
                    TotalPages = 0,
                    HasPreviousPage = false,
                    HasNextPage = false
                };

            _serviceMock
                .Setup(x =>
                    x.GetPaginationResultAsync(
                        It.IsAny<PaginationRequest>()))
                .ReturnsAsync(expectedResult);

            var result = await _controller.GetRestaurants(null);
            result.Should()
                .BeOfType<
                    OkNegotiatedContentResult<
                        PaginationResult<RestaurantDto>>>();

            _serviceMock.Verify(
                x => x.GetPaginationResultAsync(
                    It.Is<PaginationRequest>(r =>
                        r != null &&
                        r.Page == 1 &&
                        r.PageSize == 10)),
                Times.Once);
        }

        [TestMethod]
        public async Task GetRestaurants_ShouldReturnExceptionResult_WhenServiceThrowsException()
        {
            _serviceMock
                .Setup(x => x.GetPaginationResultAsync(It.IsAny<PaginationRequest>()))
                .ThrowsAsync(new Exception("Test exception"));

            var result = await _controller.GetRestaurants(
                new PaginationRequest
                {
                    Page = 1,
                    PageSize = 10
                });

            result.Should().BeOfType<ExceptionResult>();
            var exceptionResult = result.Should()
                .BeOfType<ExceptionResult>()
                .Subject;
            exceptionResult.Exception.Message
                .Should()
                .Be("Test exception");
        }

        [TestMethod]
        public async Task GetRestaurantMenu_ShouldReturnOk_WhenServiceSucceeds()
        {
            var expectedResult = new PaginationResult<MenuItemDto>
                {
                    Items = new List<MenuItemDto>
                    {
                        new MenuItemDto
                        {
                            Id = 1,
                            Name = "Paneer Tikka",
                            Price = 250,
                            PreparationTime = 20,
                            Category = "Starter",
                            QuantityAvailable = 10
                        }
                    },
                    Page = 1,
                    PageSize = 10,
                    TotalCount = 1,
                    TotalPages = 1,
                    HasPreviousPage = false,
                    HasNextPage = false
                };

            _serviceMock
                .Setup(x =>
                    x.GetRestaurantMenuAsync(
                        1,
                        It.IsAny<PaginationRequest>()))
                .ReturnsAsync(expectedResult);

            var request =
                new PaginationRequest
                {
                    Page = 1,
                    PageSize = 10
                };

            var result = await _controller.GetRestaurantMenu(1, request);
            result.Should()
                .BeOfType<
                    OkNegotiatedContentResult<
                        PaginationResult<MenuItemDto>>>();

            var okResult = result as OkNegotiatedContentResult< PaginationResult<MenuItemDto>>;
            okResult.Content
                .Should()
                .BeSameAs(expectedResult);

            _serviceMock.Verify(
                x => x.GetRestaurantMenuAsync(
                    1,
                    It.Is<PaginationRequest>(r =>
                        r.Page == 1 &&
                        r.PageSize == 10)),
                Times.Once);
        }

        [TestMethod]
        public async Task GetRestaurantMenu_ShouldUseDefaultPagination_WhenRequestIsNull()
        {
            var expectedResult =
                new PaginationResult<MenuItemDto>
                {
                    Items = new List<MenuItemDto>(),
                    Page = 1,
                    PageSize = 10,
                    TotalCount = 0,
                    TotalPages = 0,
                    HasPreviousPage = false,
                    HasNextPage = false
                };

            _serviceMock
                .Setup(x =>
                    x.GetRestaurantMenuAsync(
                        1,
                        It.IsAny<PaginationRequest>()))
                .ReturnsAsync(expectedResult);

            var result = await _controller.GetRestaurantMenu(1,null);
            result.Should()
                .BeOfType<
                    OkNegotiatedContentResult<
                        PaginationResult<MenuItemDto>>>();

            _serviceMock.Verify(
                x => x.GetRestaurantMenuAsync(
                    1,
                    It.Is<PaginationRequest>(r =>
                        r != null &&
                        r.Page == 1 &&
                        r.PageSize == 10)),
                Times.Once);
        }

        [TestMethod]
        public async Task GetRestaurantMenu_ShouldReturnExceptionResult_WhenServiceThrowsException()
        {
            _serviceMock
                .Setup(x => x.GetRestaurantMenuAsync(
                    It.IsAny<long>(),
                    It.IsAny<PaginationRequest>()))
                .ThrowsAsync(new Exception("Test exception"));

            var result = await _controller.GetRestaurantMenu(
                1,
                new PaginationRequest
                {
                    Page = 1,
                    PageSize = 10
                });

            result.Should().BeOfType<ExceptionResult>();
            var exceptionResult = result.Should()
                .BeOfType<ExceptionResult>()
                .Subject;

            exceptionResult.Exception.Message
                .Should()
                .Be("Test exception");
        }
    }
}
