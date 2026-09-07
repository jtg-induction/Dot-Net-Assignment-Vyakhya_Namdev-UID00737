using DotNetRestaurantManagement.Constants;
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
using System.Net;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Tests.Services
{
    [TestClass]
    public class RestaurantServiceTests
    {
        private Mock<IRestaurantRepository> _repositoryMock;
        private RestaurantService _service;

        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IRestaurantRepository>();
            _service = new RestaurantService(_repositoryMock.Object);
        }

        [TestMethod]
        [Description("Checks that restaurants are returned correctly with pagination and all restaurant details are mapped properly")]
        public async Task GetPaginationResultAsync_ShouldReturnRestaurants()
        {
            var restaurants = new List<Restaurant>
            {
                new Restaurant
                {
                    Id = 1,
                    Name = "Spice Garden",
                    IsActive = true,
                    Cuisine = CuisineType.Indian,
                    Address = new Address
                    {
                        City = "Pune",
                        State = "Maharashtra",
                        PinCode = "411001",
                        Country = "India"
                    }
                },
                new Restaurant
                {
                    Id = 2,
                    Name = "Dragon Wok",
                    IsActive = true,
                    Cuisine = CuisineType.Chinese,
                    Address = new Address
                    {
                        City = "Mumbai",
                        State = "Maharashtra",
                        PinCode = "400001",
                        Country = "India"
                    }
                }
            };

            _repositoryMock
                .Setup(x => x.GetActiveRestaurants())
                .Returns(CreateAsyncQueryable(restaurants));
            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 1
            };

            var result = await _service.GetRestaurantsDetailsAsync(request);
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(2);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(1);
            result.TotalPages.Should().Be(2);
            result.HasPreviousPage.Should().BeFalse();
            result.HasNextPage.Should().BeTrue();

            var restaurant = result.Items.First();
            restaurant.Id.Should().Be(1);
            restaurant.Name.Should().Be("Spice Garden");
            restaurant.Address.City.Should().Be("Pune");
            restaurant.Address.State.Should().Be("Maharashtra");
            restaurant.Address.PinCode.Should().Be("411001");
            restaurant.Address.Country.Should().Be("India");
            restaurant.Cuisine.Should().Be((CuisineType)1);
        }

        [TestMethod]
        [Description("Verifies when there are no restaurants, the service returns an empty result")]
        public async Task GetPaginationResultAsync_ShouldReturnEmptyResult_WhenNoRestaurantsExist()
        {
            var restaurants = new List<Restaurant>();
            _repositoryMock
                .Setup(x => x.GetActiveRestaurants())
                .Returns(CreateAsyncQueryable(restaurants));

            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetRestaurantsDetailsAsync(request);
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [TestMethod]
        [Description("Verify when the restaurant does not exist, the service throws Not found")]
        public async Task GetRestaurantMenuAsync_ShouldThrow_WhenRestaurantDoesNotExist()
        {
            _repositoryMock
                .Setup(x => x.GetById(999))
                .Returns((Restaurant)null);

            var request = new PaginationRequest();
            Func<Task> action = async () => await _service.GetRestaurantMenuAsync(999,request);
            var exception = await action.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.NotFound);

            exception.Which.Message
                .Should().Be(ErrorMessages.RestaurantNotFound);
        }

        [TestMethod]
        [Description("when the restaurant exists but is inactive, the service does not return its menu")]
        public async Task GetRestaurantMenuAsync_ShouldThrow_WhenRestaurantIsInactive()
        {
            var restaurant = new Restaurant
            {
                Id = 10,
                Name = "Masala Junction",
                IsActive = false
            };

            _repositoryMock
                .Setup(x => x.GetById(10))
                .Returns(restaurant);

            var request = new PaginationRequest();
            Func<Task> action = async () => await _service.GetRestaurantMenuAsync(10,request);
            var exception = await action.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.NotFound);

            exception.Which.Message
                .Should().Be(ErrorMessages.RestaurantNotFound);
        }

        private static IQueryable<T> CreateAsyncQueryable<T>(IEnumerable<T> data)
        {
            return new TestAsyncEnumerable<T>(data);
        }
    }
}
