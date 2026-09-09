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

            var result = await _service.GetPaginationResultAsync(request);
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
            restaurant.City.Should().Be("Pune");
            restaurant.State.Should().Be("Maharashtra");
            restaurant.PinCode.Should().Be("411001");
            restaurant.Country.Should().Be("India");
            restaurant.Cuisine.Should().Be("Indian");
        }

        [TestMethod]
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

            var result = await _service.GetPaginationResultAsync(request);
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [TestMethod]
        public async Task GetRestaurantMenuAsync_ShouldReturnAvailableMenuItems()
        {
            var restaurant = new Restaurant
            {
                Id = 1,
                Name = "Spice Garden",
                IsActive = true
            };

            var menuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 1,
                    RestaurantId = 1,
                    Name = "Paneer Tikka",
                    Price = 250,
                    PreparationTime = 20,
                    Category = MenuCategory.Starter,
                    QuantityAvailable = 10
                },
                new MenuItem
                {
                    Id = 2,
                    RestaurantId = 1,
                    Name = "Biryani",
                    Price = 300,
                    PreparationTime = 30,
                    Category = MenuCategory.MainCourse,
                    QuantityAvailable = 5
                }
            };

            _repositoryMock
                .Setup(x => x.GetById(1))
                .Returns(restaurant);

            _repositoryMock
                .Setup(x => x.GetAvailableMenuItems(1))
                .Returns(CreateAsyncQueryable(menuItems));

            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetRestaurantMenuAsync(1,request);
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);

            var firstItem = result.Items.First();
            firstItem.Id.Should().Be(1);
            firstItem.Name.Should().Be("Paneer Tikka");
            firstItem.Price.Should().Be(250);
            firstItem.PreparationTime.Should().Be(20);
            firstItem.Category.Should().Be("Starter");
            firstItem.QuantityAvailable.Should().Be(10);
        }

        [TestMethod]
        public async Task GetRestaurantMenuAsync_ShouldThrow_WhenRestaurantDoesNotExist()
        {
            _repositoryMock
                .Setup(x => x.GetById(999))
                .Returns((Restaurant)null);

            var request = new PaginationRequest();
            Func<Task> action = async () => await _service.GetRestaurantMenuAsync(999,request);
            await action.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Restaurant Not Found!");
        }

        [TestMethod]
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
            await action.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Restaurant Not Available!");
        }

        private static IQueryable<T> CreateAsyncQueryable<T>(IEnumerable<T> data)
        {
            return new TestAsyncEnumerable<T>(data);
        }
    }
}
