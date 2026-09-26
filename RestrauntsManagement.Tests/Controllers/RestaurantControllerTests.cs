using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Controllers;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Enums;
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
        [Description("Verify if GetRestaurants return http status 200 when service succeeds")]
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
                Address = new RestaurantAddressDto
                {
                    City = "Pune",
                    State = "Maharashtra",
                    PinCode = "411001",
                    Country = "India"
                },
                Cuisine = (CuisineType)1
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
                    x.GetRestaurantsDetailsAsync(
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
                        ApiResponse<PaginationResult<RestaurantDto>>>>();

            var okResult =
                result as OkNegotiatedContentResult<
                    ApiResponse<PaginationResult<RestaurantDto>>>;

            okResult.Content.Success.Should().BeTrue();
            okResult.Content.Data.Should().BeSameAs(expectedResult);

            _serviceMock.Verify(
                x => x.GetRestaurantsDetailsAsync(
                    It.Is<PaginationRequest>(r =>
                        r.Page == 1 &&
                        r.PageSize == 10)),
                Times.Once);
        }

        [TestMethod]
        [Description("Verify GetRestaurant uses default pagination when not specified")]
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
                    x.GetRestaurantsDetailsAsync(
                        It.IsAny<PaginationRequest>()))
                .ReturnsAsync(expectedResult);

            var result = await _controller.GetRestaurants(null);

            result.Should()
                .BeOfType<
                    OkNegotiatedContentResult<
                        ApiResponse<PaginationResult<RestaurantDto>>>>();

            var okResult =
                result as OkNegotiatedContentResult<
                    ApiResponse<PaginationResult<RestaurantDto>>>;

            okResult.Content.Success.Should().BeTrue();
            okResult.Content.Data.Should().BeSameAs(expectedResult);

            _serviceMock.Verify(
                x => x.GetRestaurantsDetailsAsync(
                    It.Is<PaginationRequest>(r =>
                        r != null &&
                        r.Page == 1 &&
                        r.PageSize == 10)),
                Times.Once);
        }

        [TestMethod]
        [Description("when the service successfully gets the menu, the controller returns 200 with correct menu")]
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
                Category = (MenuCategory)1,
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
                .Setup(x => x.GetRestaurantMenuAsync(
                    1,
                    It.IsAny<PaginationRequest>()))
                .ReturnsAsync(expectedResult);

            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 10
            };

            var result = await _controller.GetRestaurantMenu(1, request);

            result.Should()
                .BeOfType<
                    OkNegotiatedContentResult<ApiResponse<PaginationResult<MenuItemDto>>>>();

            var okResult =
                result as OkNegotiatedContentResult<ApiResponse<PaginationResult<MenuItemDto>>>;

            okResult.Content.Success.Should().BeTrue();
            okResult.Content.Data.Should().BeSameAs(expectedResult);

            _serviceMock.Verify(
                x => x.GetRestaurantMenuAsync(
                    1,
                    It.Is<PaginationRequest>(r =>
                        r.Page == 1 &&
                        r.PageSize == 10)),
                Times.Once);
        }

        [TestMethod]
        [Description("Checks when the menu pagination request is null, the controller uses the default values")]
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

            var result = await _controller.GetRestaurantMenu(1, null);

            result.Should()
                .BeOfType<
                    OkNegotiatedContentResult<
                        ApiResponse<PaginationResult<MenuItemDto>>>>();

            var okResult =
                result as OkNegotiatedContentResult<
                    ApiResponse<PaginationResult<MenuItemDto>>>;

            okResult.Content.Data
                .Should()
                .BeSameAs(expectedResult);

            okResult.Content.Success
                .Should()
                .BeTrue();

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
        [Description("Verify OnboardRestaurant returns 201 with the restaurant response when the authenticated user is a SuperAdmin")]
        public async Task OnboardRestaurant_ShouldReturnCreated_WhenUserIsSuperAdmin()
        {
            var request = new OnboardRestaurantRequest();

            var expectedResponse = new OnboardRestaurantResponse
            {
                restaurantId = 1
            };

            _serviceMock
                .Setup(x => x.OnboardRestaurant(request))
                .ReturnsAsync(expectedResponse);

            var identity = new System.Security.Claims.ClaimsIdentity("TestAuth");

            identity.AddClaim(
                new System.Security.Claims.Claim(
                    StringConstants.UserId,
                    "1"));

            identity.AddClaim(
                new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.Role,
                    UserRole.SuperAdmin.ToString()));

            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            _controller.RequestContext.Principal = principal;
            _controller.User = principal;
            System.Threading.Thread.CurrentPrincipal = principal;

            var result = await _controller.OnboardRestaurant(request);

            result.Should()
                .BeOfType<CreatedNegotiatedContentResult<
                    ApiResponse<OnboardRestaurantResponse>>>();

            var okResult =
                result as CreatedNegotiatedContentResult<
                    ApiResponse<OnboardRestaurantResponse>>;

            okResult.Content.Success.Should().BeTrue();
            okResult.Content.Data.Should().BeSameAs(expectedResponse);
            okResult.Content.Message.Should().Be(SuccessMessages.RestaurantOnboarded);

            _serviceMock.Verify(
                x => x.OnboardRestaurant(request),
                Times.Once);
        }
    }
}
