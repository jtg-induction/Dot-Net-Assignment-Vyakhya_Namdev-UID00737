using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories;
using DotNetRestaurantManagement.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace RestrauntsManagement.Tests.Repository
{
    [TestClass]
    public class OrderRepositoryTests
    {
        private Mock<RestaurantDbContext> _contextMock;
        private OrderRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            _contextMock = new Mock<RestaurantDbContext>();
            _repository = new OrderRepository(_contextMock.Object);
        }

        [TestMethod]
        [Description("Checks provided delivery Address should belongs to user")]
        public async Task IsAddressBelongsToUserAsync_ShouldReturnTrue_WhenAddressBelongsToUser()
        {
            var userAddresses = new List<UserAddress>
            {
                new UserAddress
                {
                    UserId = 1,
                    AddressId = 10
                },
                new UserAddress
                {
                    UserId = 2,
                    AddressId = 20
                }
            };

            var userAddressesMock = CreateMockDbSet(userAddresses);

            _contextMock
                .Setup(x => x.UserAddresses)
                .Returns(userAddressesMock.Object);

            var result = await _repository.IsAddressBelongsToUserAsync(1, 10);

            result.Should().BeTrue();
        }

        [TestMethod]
        [Description("Checks IsAddressBelongsToUserAsync should return false if address don't belongs to user")]
        public async Task IsAddressBelongsToUserAsync_ShouldReturnFalse_WhenAddressDoesNotBelongToUser()
        {
            var userAddresses = new List<UserAddress>
            {
                new UserAddress
                {
                    UserId = 1,
                    AddressId = 10
                }
            };

            var userAddressesMock = CreateMockDbSet(userAddresses);

            _contextMock
                .Setup(x => x.UserAddresses)
                .Returns(userAddressesMock.Object);

            var result = await _repository.IsAddressBelongsToUserAsync(1, 20);

            result.Should().BeFalse();
        }

        [TestMethod]
        [Description("Verify this should return MenuItems when MenuItem belongs to Restaurant")]
        public async Task GetMenuItemAsync_ShouldReturnMenuItem_WhenMenuItemBelongsToRestaurant()
        {
            var menuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 32,
                    RestaurantId = 13,
                    Name = "Spring Rolls",
                    Price = 180,
                    QuantityAvailable = 50
                },
                new MenuItem
                {
                    Id = 37,
                    RestaurantId = 14,
                    Name = "Garlic Bread",
                    Price = 150,
                    QuantityAvailable = 50
                }
            };

            var menuItemsMock = CreateMockDbSet(menuItems);

            _contextMock
                .Setup(x => x.MenuItems)
                .Returns(menuItemsMock.Object);

            var result = await _repository.GetMenuItemAsync(32, 13);

            result.Should().NotBeNull();
            result.Id.Should().Be(32);
            result.RestaurantId.Should().Be(13);
            result.Name.Should().Be("Spring Rolls");
        }

        [TestMethod]
        [Description("Verify GetMenuItemAsync should return null when item does not belongs to restaurant")]
        public async Task GetMenuItemAsync_ShouldReturnNull_WhenMenuItemDoesNotBelongToRestaurant()
        {
            var menuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 32,
                    RestaurantId = 13,
                    Name = "Spring Rolls",
                    Price = 180,
                    QuantityAvailable = 50
                }
            };

            var menuItemsMock = CreateMockDbSet(menuItems);

            _contextMock
                .Setup(x => x.MenuItems)
                .Returns(menuItemsMock.Object);

            var result = await _repository.GetMenuItemAsync(32, 14);

            result.Should().BeNull();
        }

        [TestMethod]
        [Description("Check AddOrder should add order to DB")]
        public void AddOrder_ShouldAddOrderToContext()
        {
            var order = new Order
            {
                Id = 1,
                CustomerId = 1,
                RestaurantId = 13,
                DeliveryAddressId = 1,
                TotalItems = 2,
                TotalAmount = 400
            };

            var ordersMock = new Mock<DbSet<Order>>();

            _contextMock
                .Setup(x => x.Orders)
                .Returns(ordersMock.Object);

            _repository.AddOrder(order);

            ordersMock.Verify(
                x => x.Add(order),
                Times.Once);
        }

        [TestMethod]
        [Description("Verify AddOrderItem should add order_item to DB")]
        public void AddOrderItem_ShouldAddOrderItemToContext()
        {
            var orderItem = new OrderItem
            {
                Id = 1,
                OrderId = 1,
                MenuItemId = 32,
                Quantity = 2,
                Price = 180
            };

            var orderItemsMock = new Mock<DbSet<OrderItem>>();

            _contextMock
                .Setup(x => x.OrderedItems)
                .Returns(orderItemsMock.Object);

            _repository.AddOrderItem(orderItem);

            orderItemsMock.Verify(
                x => x.Add(orderItem),
                Times.Once);
        }

        [TestMethod]
        [Description("Verify SaveChangesAsync should call context save changes method")]
        public async Task SaveChangesAsync_ShouldCallContextSaveChanges()
        {
            _contextMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            await _repository.SaveChangesAsync();

            _contextMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(
            List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IDbAsyncEnumerable<T>>()
                .Setup(x => x.GetAsyncEnumerator())
                .Returns(new TestAsyncEnumerator<T>(
                    queryable.GetEnumerator()));

            mockSet.As<IQueryable<T>>()
                .Setup(x => x.Provider)
                .Returns(new TestAsyncQueryProvider<T>(
                    queryable.Provider));

            mockSet.As<IQueryable<T>>()
                .Setup(x => x.Expression)
                .Returns(queryable.Expression);

            mockSet.As<IQueryable<T>>()
                .Setup(x => x.ElementType)
                .Returns(queryable.ElementType);

            mockSet.As<IQueryable<T>>()
                .Setup(x => x.GetEnumerator())
                .Returns(queryable.GetEnumerator());

            return mockSet;
        }
    }
}