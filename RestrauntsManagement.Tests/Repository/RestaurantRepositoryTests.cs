using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories.Implementations;
using Effort;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Web.UI.WebControls;

namespace DotNetRestaurantManagement.Tests.Repositories
{
    [TestClass]
    public class RestaurantRepositoryTests
    {
        private RestaurantDbContext _context;
        private RestaurantRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var connection = DbConnectionFactory.CreateTransient();
            _context = new RestaurantDbContext(connection);
            _context.Users.Add(new DotNetRestaurantManagement.Models.Entities.User
            {
                Name = "Test Owner",
                Email = "owner@test.com",
                Password = "HASH",
                PhoneNumber = "9999999999"
            });
            _context.Addresses.Add(new DotNetRestaurantManagement.Models.Entities.Address
            {
                HouseNumber = "1",
                StreetAddress = "Test Street",
                City = "Delhi",
                State = "Delhi",
                PinCode = "110001",
                Country = "India"
            });

            _context.SaveChanges();
            _repository = new RestaurantRepository(_context);
        }

        private Restaurant CreateRestaurant(string name, string email, bool isActive)
        {
            return new Restaurant
            {
                Name = name,
                Email = email,
                OwnerId = _context.Users.First().Id,
                AddressId = _context.Addresses.First().Id,
                IsActive = isActive
            };
        }

        [TestMethod]
        public void GetActiveRestaurants_ShouldReturnOnlyActiveRestaurants()
        {
            _context.Restaurants.Add(
                CreateRestaurant(
                    "Spice Garden",
                    "spice@test.com",
                    true));

            _context.Restaurants.Add(
                CreateRestaurant(
                    "Dragon Wok",
                    "dragon@test.com",
                    false));

            _context.Restaurants.Add(
                CreateRestaurant(
                    "Bella Italia",
                    "bella@test.com",
                    true));

            _context.SaveChanges();
            var result = _repository
                .GetActiveRestaurants()
                .ToList();

            result.Should().HaveCount(2);
            result.Should().OnlyContain(x => x.IsActive);
            result
                .Select(x => x.Name)
                .Should()
                .BeEquivalentTo(
                    "Spice Garden",
                    "Bella Italia");
        }

        [TestMethod]
        public void GetActiveRestaurants_ShouldReturnEmpty_WhenNoRestaurantIsActive()
        {
            _context.Restaurants.Add(
                CreateRestaurant(
                    "Spice Garden",
                    "spice@test.com",
                    false));

            _context.Restaurants.Add(
                CreateRestaurant(
                    "Dragon Wok",
                    "dragon@test.com",
                    false));

            _context.SaveChanges();
            var result = _repository
                .GetActiveRestaurants()
                .ToList();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetActiveRestaurants_ShouldReturnAllRestaurants_WhenAllAreActive()
        {
            _context.Restaurants.Add(
                CreateRestaurant(
                    "Spice Garden",
                    "spice@test.com",
                    true));

            _context.Restaurants.Add(
                CreateRestaurant(
                    "Dragon Wok",
                    "dragon@test.com",
                    true));

            _context.Restaurants.Add(
                CreateRestaurant(
                    "Bella Italia",
                    "bella@test.com",
                    true));

            _context.SaveChanges();
            var result = _repository
                .GetActiveRestaurants()
                .ToList();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(x => x.IsActive);
        }

        [TestMethod]
        public void GetAvailableMenuItems_ShouldReturnEmpty_WhenRestaurantHasNoMenuItems()
        {
            var restaurant = CreateRestaurant(
                "Spice Garden",
                "spice@test.com",
                true);

            _context.Restaurants.Add(restaurant);
            _context.SaveChanges();
            var result = _repository
                .GetAvailableMenuItems(restaurant.Id)
                .ToList();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetById_ShouldReturnRestaurant_WhenRestaurantExists()
        {
            var restaurant = CreateRestaurant(
                "Dragon Wok",
                "dragon@test.com",
                true);

            _context.Restaurants.Add(restaurant);
            _context.SaveChanges();
            var result = _repository.GetById(restaurant.Id);
            result.Should().NotBeNull();
            result.Id.Should().Be(restaurant.Id);
            result.Name.Should().Be("Dragon Wok");
        }

        [TestMethod]
        public void GetById_ShouldReturnNull_WhenRestaurantDoesNotExist()
        {
            var restaurant = CreateRestaurant(
                "Spice Garden",
                "spice@test.com",
                true);

            _context.Restaurants.Add(restaurant);
            _context.SaveChanges();
            var result = _repository.GetById(999);
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetById_ShouldReturnInactiveRestaurant_WhenRestaurantExists()
        {
            var restaurant = CreateRestaurant(
                "Royal Spice",
                "royal@test.com",
                false);

            _context.Restaurants.Add(restaurant);
            _context.SaveChanges();
            var result = _repository.GetById(restaurant.Id);
            result.Should().NotBeNull();
            result.Id.Should().Be(restaurant.Id);
            result.IsActive.Should().BeFalse();
        }
    }
}
