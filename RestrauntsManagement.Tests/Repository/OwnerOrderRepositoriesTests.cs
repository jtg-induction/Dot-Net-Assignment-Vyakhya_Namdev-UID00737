using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories;
using DotNetRestaurantManagement.Repositories.Implementations;
using Effort;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Tests.Repositories
{
    [TestClass]
    public class OwnerOrderRepositoryTests
    {
        private RestaurantDbContext _context;
        private OwnerOrderRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var connection = DbConnectionFactory.CreateTransient();
            _context = new RestaurantDbContext(connection);
            _repository = new OwnerOrderRepository(_context);
        }

        private User CreateUser(long id, string email, UserRole role)
        {
            return new User
            {
                Id = id,
                Email = email,
                Name = role == UserRole.Owner
                    ? "Test Owner"
                    : "Test Customer",
                Password = "HASH",
                PhoneNumber = id.ToString().PadLeft(10, '9'),
                Role = role,
                IsActive = true
            };
        }

        private Address CreateAddress()
        {
            return new Address
            {
                HouseNumber = "101",
                StreetAddress = "Test Street",
                City = "Delhi",
                State = "Delhi",
                PinCode = "110001",
                Country = "India",
                AddressType = AddressType.Other
            };
        }

        private Restaurant CreateRestaurant(
            long ownerId,
            long addressId,
            string email)
        {
            return new Restaurant
            {
                Email = email,
                OwnerId = ownerId,
                Name = "Test Restaurant",
                AddressId = addressId,
                IsActive = true
            };
        }

        private Order CreateOrder(
            long restaurantId,
            long customerId,
            long deliveryAddressId)
        {
            return new Order
            {
                RestaurantId = restaurantId,
                CustomerId = customerId,
                DeliveryAddressId = deliveryAddressId,
                TotalItems = 2,
                TotalAmount = 500,
                Status = OrderStatus.Placed
            };
        }

        [TestMethod]
        [Description("Should return orders belonging to the specified owner")]
        public async Task GetOrders_WhenOwnerHasOrders_ReturnsOwnerOrders()
        {
            var owner = CreateUser(
                100,
                "owner@test.com",
                UserRole.Owner);

            var customer = CreateUser(
                200,
                "customer@test.com",
                UserRole.Customer);

            var restaurantAddress = CreateAddress();
            var deliveryAddress = CreateAddress();

            _context.Users.Add(owner);
            _context.Users.Add(customer);
            _context.Addresses.Add(restaurantAddress);
            _context.Addresses.Add(deliveryAddress);

            await _context.SaveChangesAsync();

            var restaurant = CreateRestaurant(
                owner.Id,
                restaurantAddress.Id,
                "restaurant@test.com");

            _context.Restaurants.Add(restaurant);

            await _context.SaveChangesAsync();

            var order = CreateOrder(
                restaurant.Id,
                customer.Id,
                deliveryAddress.Id);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            var result = _repository
                .GetOrders(owner.Id)
                .ToList();

            result.Should().HaveCount(1);
            result.First().Id.Should().Be(order.Id);
            result.First().RestaurantId.Should().Be(restaurant.Id);
        }

        [TestMethod]
        [Description("Should return only orders belonging to the specified owner")]
        public async Task GetOrders_WhenMultipleOwnersExist_ReturnsOnlyOwnerOrders()
        {
            var owner1 = CreateUser(
                100,
                "owner1@test.com",
                UserRole.Owner);

            var owner2 = CreateUser(
                101,
                "owner2@test.com",
                UserRole.Owner);

            var customer = CreateUser(
                200,
                "customer@test.com",
                UserRole.Customer);

            var restaurantAddress1 = CreateAddress();
            var restaurantAddress2 = CreateAddress();

            var deliveryAddress1 = CreateAddress();
            var deliveryAddress2 = CreateAddress();

            _context.Users.Add(owner1);
            _context.Users.Add(owner2);
            _context.Users.Add(customer);

            _context.Addresses.Add(restaurantAddress1);
            _context.Addresses.Add(restaurantAddress2);
            _context.Addresses.Add(deliveryAddress1);
            _context.Addresses.Add(deliveryAddress2);

            await _context.SaveChangesAsync();

            var restaurant1 = CreateRestaurant(
                owner1.Id,
                restaurantAddress1.Id,
                "restaurant1@test.com");

            var restaurant2 = CreateRestaurant(
                owner2.Id,
                restaurantAddress2.Id,
                "restaurant2@test.com");

            _context.Restaurants.Add(restaurant1);
            _context.Restaurants.Add(restaurant2);

            await _context.SaveChangesAsync();

            var order1 = CreateOrder(
                restaurant1.Id,
                customer.Id,
                deliveryAddress1.Id);

            var order2 = CreateOrder(
                restaurant2.Id,
                customer.Id,
                deliveryAddress2.Id);

            _context.Orders.Add(order1);
            _context.Orders.Add(order2);

            await _context.SaveChangesAsync();
            var result = _repository
                .GetOrders(owner1.Id)
                .ToList();

            result.Should().HaveCount(1);
            result.First().Id.Should().Be(order1.Id);
            result.First().RestaurantId.Should().Be(restaurant1.Id);
        }

        [TestMethod]
        [Description("Should return empty result when owner has no orders")]
        public void GetOrders_WhenOwnerHasNoOrders_ReturnsEmpty()
        {
            var owner = CreateUser(
                100,
                "owner@test.com",
                UserRole.Owner);

            _context.Users.Add(owner);
            _context.SaveChanges();

            var result = _repository
                .GetOrders(owner.Id)
                .ToList();

            result.Should().BeEmpty();
        }

        [TestMethod]
        [Description("Should return order when owner, restaurant and order IDs match")]
        public async Task GetOrderForStatusUpdateAsync_WhenAllIdsMatch_ReturnsOrder()
        {
            var owner = CreateUser(
                100,
                "owner@test.com",
                UserRole.Owner);

            var customer = CreateUser(
                200,
                "customer@test.com",
                UserRole.Customer);

            var restaurantAddress = CreateAddress();
            var deliveryAddress = CreateAddress();

            _context.Users.Add(owner);
            _context.Users.Add(customer);
            _context.Addresses.Add(restaurantAddress);
            _context.Addresses.Add(deliveryAddress);

            await _context.SaveChangesAsync();

            var restaurant = CreateRestaurant(
                owner.Id,
                restaurantAddress.Id,
                "restaurant@test.com");

            _context.Restaurants.Add(restaurant);

            await _context.SaveChangesAsync();

            var order = CreateOrder(
                restaurant.Id,
                customer.Id,
                deliveryAddress.Id);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            var result = await _repository.GetOrderForStatusUpdateAsync(
                owner.Id,
                restaurant.Id,
                order.Id);

            result.Should().NotBeNull();
            result.Id.Should().Be(order.Id);
            result.RestaurantId.Should().Be(restaurant.Id);
        }

        [TestMethod]
        [Description("Should return null when order does not exist")]
        public async Task GetOrderForStatusUpdateAsync_WhenOrderDoesNotExist_ReturnsNull()
        {
            var owner = CreateUser(
                100,
                "owner@test.com",
                UserRole.Owner);

            var restaurantAddress = CreateAddress();

            _context.Users.Add(owner);
            _context.Addresses.Add(restaurantAddress);
            await _context.SaveChangesAsync();
            var restaurant = CreateRestaurant(
                owner.Id,
                restaurantAddress.Id,
                "restaurant@test.com");

            _context.Restaurants.Add(restaurant);

            await _context.SaveChangesAsync();
            var result = await _repository.GetOrderForStatusUpdateAsync(
                owner.Id,
                restaurant.Id,
                999);

            result.Should().BeNull();
        }

        [TestMethod]
        [Description("Should return null when owner ID does not match")]
        public async Task GetOrderForStatusUpdateAsync_WhenOwnerDoesNotMatch_ReturnsNull()
        {
            var owner = CreateUser(
                100,
                "owner@test.com",
                UserRole.Owner);

            var otherOwner = CreateUser(
                101,
                "otherowner@test.com",
                UserRole.Owner);

            var customer = CreateUser(
                200,
                "customer@test.com",
                UserRole.Customer);

            var restaurantAddress = CreateAddress();
            var deliveryAddress = CreateAddress();

            _context.Users.Add(owner);
            _context.Users.Add(otherOwner);
            _context.Users.Add(customer);

            _context.Addresses.Add(restaurantAddress);
            _context.Addresses.Add(deliveryAddress);

            await _context.SaveChangesAsync();

            var restaurant = CreateRestaurant(
                owner.Id,
                restaurantAddress.Id,
                "restaurant@test.com");

            _context.Restaurants.Add(restaurant);

            await _context.SaveChangesAsync();
            var order = CreateOrder(
                restaurant.Id,
                customer.Id,
                deliveryAddress.Id);

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();
            var result = await _repository.GetOrderForStatusUpdateAsync(
                otherOwner.Id,
                restaurant.Id,
                order.Id);

            result.Should().BeNull();
        }

        [TestMethod]
        [Description("Should return null when restaurant ID does not match")]
        public async Task GetOrderForStatusUpdateAsync_WhenRestaurantDoesNotMatch_ReturnsNull()
        {
            var owner = CreateUser(
                100,
                "owner@test.com",
                UserRole.Owner);

            var customer = CreateUser(
                200,
                "customer@test.com",
                UserRole.Customer);

            var restaurantAddress1 = CreateAddress();
            var restaurantAddress2 = CreateAddress();
            var deliveryAddress = CreateAddress();

            _context.Users.Add(owner);
            _context.Users.Add(customer);

            _context.Addresses.Add(restaurantAddress1);
            _context.Addresses.Add(restaurantAddress2);
            _context.Addresses.Add(deliveryAddress);

            await _context.SaveChangesAsync();
            var restaurant1 = CreateRestaurant(
                owner.Id,
                restaurantAddress1.Id,
                "restaurant1@test.com");

            var restaurant2 = CreateRestaurant(
                owner.Id,
                restaurantAddress2.Id,
                "restaurant2@test.com");

            _context.Restaurants.Add(restaurant1);
            _context.Restaurants.Add(restaurant2);

            await _context.SaveChangesAsync();
            var order = CreateOrder(
                restaurant1.Id,
                customer.Id,
                deliveryAddress.Id);

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();
            var result = await _repository.GetOrderForStatusUpdateAsync(
                owner.Id,
                restaurant2.Id,
                order.Id);

            result.Should().BeNull();
        }

        [TestMethod]
        [Description("Should return null when order ID does not match")]
        public async Task GetOrderForStatusUpdateAsync_WhenOrderIdDoesNotMatch_ReturnsNull()
        {
            var owner = CreateUser(
                100,
                "owner@test.com",
                UserRole.Owner);

            var customer = CreateUser(
                200,
                "customer@test.com",
                UserRole.Customer);

            var restaurantAddress = CreateAddress();
            var deliveryAddress = CreateAddress();

            _context.Users.Add(owner);
            _context.Users.Add(customer);

            _context.Addresses.Add(restaurantAddress);
            _context.Addresses.Add(deliveryAddress);

            await _context.SaveChangesAsync();

            var restaurant = CreateRestaurant(
                owner.Id,
                restaurantAddress.Id,
                "restaurant@test.com");

            _context.Restaurants.Add(restaurant);

            await _context.SaveChangesAsync();

            var order = CreateOrder(
                restaurant.Id,
                customer.Id,
                deliveryAddress.Id);

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            var result = await _repository.GetOrderForStatusUpdateAsync(
                owner.Id,
                restaurant.Id,
                order.Id + 999);

            result.Should().BeNull();
        }

        [TestMethod]
        [Description("Should persist changes using SaveChangesAsync")]
        public async Task SaveChangesAsync_ShouldPersistChanges()
        {
            var owner = CreateUser(
                100,
                "owner@test.com",
                UserRole.Owner);

            _context.Users.Add(owner);

            await _context.SaveChangesAsync();

            owner.Name = "Updated Owner";

            await _repository.SaveChangesAsync();

            var result = await _context.Users
                .FirstAsync(x => x.Id == owner.Id);

            result.Name.Should().Be("Updated Owner");
        }

        [TestMethod]
        [Description("Should return orders from all restaurants belonging to the owner")]
        public async Task GetOrders_WhenOwnerHasMultipleRestaurants_ReturnsAllOwnerOrders()
        {
            var owner = CreateUser(
                100,
                "owner@test.com",
                UserRole.Owner);

            var customer = CreateUser(
                200,
                "customer@test.com",
                UserRole.Customer);

            var restaurantAddress1 = CreateAddress();
            var restaurantAddress2 = CreateAddress();

            var deliveryAddress1 = CreateAddress();
            var deliveryAddress2 = CreateAddress();

            _context.Users.Add(owner);
            _context.Users.Add(customer);

            _context.Addresses.Add(restaurantAddress1);
            _context.Addresses.Add(restaurantAddress2);
            _context.Addresses.Add(deliveryAddress1);
            _context.Addresses.Add(deliveryAddress2);

            await _context.SaveChangesAsync();

            var restaurant1 = CreateRestaurant(
                owner.Id,
                restaurantAddress1.Id,
                "restaurant1@test.com");

            var restaurant2 = CreateRestaurant(
                owner.Id,
                restaurantAddress2.Id,
                "restaurant2@test.com");

            _context.Restaurants.Add(restaurant1);
            _context.Restaurants.Add(restaurant2);

            await _context.SaveChangesAsync();

            var order1 = CreateOrder(
                restaurant1.Id,
                customer.Id,
                deliveryAddress1.Id);

            var order2 = CreateOrder(
                restaurant2.Id,
                customer.Id,
                deliveryAddress2.Id);

            _context.Orders.Add(order1);
            _context.Orders.Add(order2);

            await _context.SaveChangesAsync();

            var result = _repository
                .GetOrders(owner.Id)
                .ToList();

            result.Should().HaveCount(2);
            result.Should().Contain(x => x.Id == order1.Id);
            result.Should().Contain(x => x.Id == order2.Id);
        }
    }
}
