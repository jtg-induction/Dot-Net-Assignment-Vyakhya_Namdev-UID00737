using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Implementations;
using DotNetRestaurantManagement.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Tests.Repositories
{
    [TestClass]
    public class ReportRepositoryTests
    {
        private static ReportRepository CreateFrequentlyBoughtRepository(
            List<OrderItem> orderItems,
            List<MenuItem> menuItems)
        {
            var orderItemsDbSet = CreateMockDbSet(orderItems);
            var menuItemsDbSet = CreateMockDbSet(menuItems);

            var context = new RestaurantDbContext();

            context.OrderItems = orderItemsDbSet.Object;
            context.MenuItems = menuItemsDbSet.Object;

            return new ReportRepository(context);
        }

        [TestMethod]
        [Description("Verifies that top ordered items are returned only for the specified owner and delivered orders")]
        public async Task GetTopOrderedItems_ValidOwner_ReturnsOnlyDeliveredItemsForOwner()
        {
            var restaurant1 = CreateRestaurant(1, 101, "Restaurant 1");
            var restaurant2 = CreateRestaurant(2, 202, "Restaurant 2");

            var item1 = CreateMenuItem(1, "Burger");
            var item2 = CreateMenuItem(2, "Pizza");

            var order1 = CreateOrder(1, 1, restaurant1, OrderStatus.Delivered);
            var order2 = CreateOrder(2, 2, restaurant2, OrderStatus.Delivered);
            var order3 = CreateOrder(3, 1, restaurant1, OrderStatus.Dispatched);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order1, item1, 5),
                CreateOrderItem(2, order2, item2, 10),
                CreateOrderItem(3, order3, item1, 20)
            };

            var repository = CreateRepository(orderItems);

            var result = await repository.GetTopOrderedItems(101, null, null);

            result.Should().ContainSingle();
            result[0].RestaurantId.Should().Be(1);
            result[0].RestaurantName.Should().Be("Restaurant 1");
            result[0].MenuItemId.Should().Be(1);
            result[0].MenuItemName.Should().Be("Burger");
            result[0].NumberOfTimesOrdered.Should().Be(5);
        }

        [TestMethod]
        [Description("Verifies that specifying a restaurant ID filters the result to that restaurant")]
        public async Task GetTopOrderedItems_RestaurantIdProvided_ReturnsOnlySpecifiedRestaurantItems()
        {
            var restaurant1 = CreateRestaurant(1, 101, "Restaurant 1");
            var restaurant2 = CreateRestaurant(2, 101, "Restaurant 2");

            var item1 = CreateMenuItem(1, "Burger");
            var item2 = CreateMenuItem(2, "Pizza");

            var order1 = CreateOrder(1, 1, restaurant1, OrderStatus.Delivered);
            var order2 = CreateOrder(2, 2, restaurant2, OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order1, item1, 5),
                CreateOrderItem(2, order2, item2, 10)
            };

            var repository = CreateRepository(orderItems);

            var result = await repository.GetTopOrderedItems(101, 1, null);

            result.Should().ContainSingle();
            result[0].RestaurantId.Should().Be(1);
            result[0].MenuItemId.Should().Be(1);
            result[0].NumberOfTimesOrdered.Should().Be(5);
        }

        [TestMethod]
        [Description("Verifies that excluded menu item IDs are removed from the result")]
        public async Task GetTopOrderedItems_ExcludedItemIdsProvided_DoesNotReturnExcludedItems()
        {
            var restaurant = CreateRestaurant(1, 101, "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");
            var pizza = CreateMenuItem(2, "Pizza");

            var order = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order, burger, 10),
                CreateOrderItem(2, order, pizza, 5)
            };

            var repository = CreateRepository(orderItems);

            var result = await repository.GetTopOrderedItems(
                101,
                null,
                new List<long> { 1 });

            result.Should().ContainSingle();
            result[0].MenuItemId.Should().Be(2);
            result[0].MenuItemName.Should().Be("Pizza");
        }

        [TestMethod]
        [Description("Verifies that a null excluded item collection is handled correctly")]
        public async Task GetTopOrderedItems_NullExcludedItemIds_ReturnsAllEligibleItems()
        {
            var restaurant = CreateRestaurant(1, 101, "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");
            var pizza = CreateMenuItem(2, "Pizza");

            var order = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order, burger, 10),
                CreateOrderItem(2, order, pizza, 5)
            };

            var repository = CreateRepository(orderItems);

            var result = await repository.GetTopOrderedItems(101, null, null);

            result.Should().HaveCount(2);
            result.Select(x => x.MenuItemId)
                .Should()
                .Contain(new long[] { 1, 2 });
        }

        [TestMethod]
        [Description("Verifies that multiple orders for the same menu item are aggregated by quantity")]
        public async Task GetTopOrderedItems_SameMenuItemOrderedMultipleTimes_SumsQuantities()
        {
            var restaurant = CreateRestaurant(1, 101, "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");

            var order1 = CreateOrder(1, 1, restaurant, OrderStatus.Delivered);
            var order2 = CreateOrder(2, 1, restaurant, OrderStatus.Delivered);
            var order3 = CreateOrder(3, 1, restaurant, OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order1, burger, 3),
                CreateOrderItem(2, order2, burger, 5),
                CreateOrderItem(3, order3, burger, 2)
            };

            var repository = CreateRepository(orderItems);

            var result = await repository.GetTopOrderedItems(101, null, null);

            result.Should().ContainSingle();
            result[0].MenuItemId.Should().Be(1);
            result[0].NumberOfTimesOrdered.Should().Be(10);
        }

        [TestMethod]
        [Description("Verifies that results are ordered by total quantity in descending order")]
        public async Task GetTopOrderedItems_MultipleItems_ReturnsItemsOrderedByQuantityDescending()
        {
            var restaurant = CreateRestaurant(1, 101, "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");
            var pizza = CreateMenuItem(2, "Pizza");
            var pasta = CreateMenuItem(3, "Pasta");

            var order = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order, burger, 5),
                CreateOrderItem(2, order, pizza, 15),
                CreateOrderItem(3, order, pasta, 10)
            };

            var repository = CreateRepository(orderItems);

            var result = await repository.GetTopOrderedItems(101, null, null);

            result.Should().HaveCount(3);
            result[0].MenuItemId.Should().Be(2);
            result[0].NumberOfTimesOrdered.Should().Be(15);
            result[1].MenuItemId.Should().Be(3);
            result[1].NumberOfTimesOrdered.Should().Be(10);
            result[2].MenuItemId.Should().Be(1);
            result[2].NumberOfTimesOrdered.Should().Be(5);
        }

        [TestMethod]
        [Description("Verifies that no more than the top ten menu items are returned")]
        public async Task GetTopOrderedItems_MoreThanTenItems_ReturnsOnlyTopTen()
        {
            var restaurant = CreateRestaurant(1, 101, "Restaurant 1");

            var order = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var orderItems = new List<OrderItem>();

            for (long i = 1; i <= 15; i++)
            {
                var menuItem = CreateMenuItem(i, "Item " + i);
                orderItems.Add(
                    CreateOrderItem(i, order, menuItem, (int)i));
            }

            var repository = CreateRepository(orderItems);

            var result = await repository.GetTopOrderedItems(101, null, null);

            result.Should().HaveCount(10);

            result.Select(x => x.MenuItemId)
                .Should()
                .Equal(
                    15,
                    14,
                    13,
                    12,
                    11,
                    10,
                    9,
                    8,
                    7,
                    6);
        }

        [TestMethod]
        [Description("Verifies that an empty excluded item collection returns all eligible items")]
        public async Task GetTopOrderedItems_EmptyExcludedItemIds_ReturnsAllEligibleItems()
        {
            var restaurant = CreateRestaurant(1, 101, "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");
            var pizza = CreateMenuItem(2, "Pizza");

            var order = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order, burger, 10),
                CreateOrderItem(2, order, pizza, 5)
            };

            var repository = CreateRepository(orderItems);

            var result = await repository.GetTopOrderedItems(
                101,
                null,
                new List<long>());

            result.Should().HaveCount(2);

            result.Select(x => x.MenuItemId)
                .Should()
                .Contain(new long[] { 1, 2 });
        }

        [TestMethod]
        [Description("Verifies that no matching owner returns an empty result")]
        public async Task GetTopOrderedItems_NoMatchingOwner_ReturnsEmptyResult()
        {
            var restaurant = CreateRestaurant(1, 101, "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");

            var order = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order, burger, 10)
            };

            var repository = CreateRepository(orderItems);

            var result = await repository.GetTopOrderedItems(
                999,
                null,
                null);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        [Description("Verifies that size 3 returns the most frequently ordered triplet.")]
        public async Task GetFrequentlyBoughtTogether_SizeThree_ReturnsMostFrequentlyOrderedTriplet()
        {
            var restaurant = CreateRestaurant(
                1,
                101,
                "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");
            var pizza = CreateMenuItem(2, "Pizza");
            var pasta = CreateMenuItem(3, "Pasta");
            var coke = CreateMenuItem(4, "Coke");

            var order1 = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var order2 = CreateOrder(
                2,
                1,
                restaurant,
                OrderStatus.Delivered);

            var order3 = CreateOrder(
                3,
                1,
                restaurant,
                OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order1, burger, 1),
                CreateOrderItem(2, order1, pizza, 1),
                CreateOrderItem(3, order1, pasta, 1),

                CreateOrderItem(4, order2, burger, 1),
                CreateOrderItem(5, order2, pizza, 1),
                CreateOrderItem(6, order2, pasta, 1),

                CreateOrderItem(7, order3, burger, 1),
                CreateOrderItem(8, order3, pizza, 1),
                CreateOrderItem(9, order3, coke, 1)
            };

            var repository = CreateFrequentlyBoughtRepository(
                orderItems,
                new List<MenuItem>
                {
                    burger,
                    pizza,
                    pasta,
                    coke
                });

            var result = await repository.GetFrequentlyBoughtTogether(
                1,
                3);

            result.Should().ContainSingle();

            result[0].Item1.Should().Be("Burger");
            result[0].Item2.Should().Be("Pizza");
            result[0].Item3.Should().Be("Pasta");
            result[0].ItemCount.Should().Be(3);
            result[0].NumberOfTimesBought.Should().Be(2);
        }

        [TestMethod]
        [Description("Verifies that only delivered orders are considered when calculating triplets.")]
        public async Task GetFrequentlyBoughtTogether_SizeThree_IgnoresNonDeliveredOrders()
        {
            var restaurant = CreateRestaurant(
                1,
                101,
                "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");
            var pizza = CreateMenuItem(2, "Pizza");
            var pasta = CreateMenuItem(3, "Pasta");

            var deliveredOrder = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var dispatchedOrder = CreateOrder(
                2,
                1,
                restaurant,
                OrderStatus.Dispatched);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, deliveredOrder, burger, 1),
                CreateOrderItem(2, deliveredOrder, pizza, 1),
                CreateOrderItem(3, deliveredOrder, pasta, 1),

                CreateOrderItem(4, dispatchedOrder, burger, 1),
                CreateOrderItem(5, dispatchedOrder, pizza, 1),
                CreateOrderItem(6, dispatchedOrder, pasta, 1)
            };

            var repository = CreateFrequentlyBoughtRepository(
                orderItems,
                new List<MenuItem>
                {
                    burger,
                    pizza,
                    pasta
                });

            var result = await repository.GetFrequentlyBoughtTogether(
                1,
                3);

            result.Should().ContainSingle();

            result[0].Item1.Should().Be("Burger");
            result[0].Item2.Should().Be("Pizza");
            result[0].Item3.Should().Be("Pasta");
            result[0].NumberOfTimesBought.Should().Be(1);
        }

        [TestMethod]
        [Description("Verifies that size 2 returns the most frequently ordered pair.")]
        public async Task GetFrequentlyBoughtTogether_SizeTwo_ReturnsMostFrequentlyOrderedPair()
        {
            var restaurant = CreateRestaurant(
                1,
                101,
                "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");
            var pizza = CreateMenuItem(2, "Pizza");
            var pasta = CreateMenuItem(3, "Pasta");

            var order1 = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var order2 = CreateOrder(
                2,
                1,
                restaurant,
                OrderStatus.Delivered);

            var order3 = CreateOrder(
                3,
                1,
                restaurant,
                OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order1, burger, 1),
                CreateOrderItem(2, order1, pizza, 1),
                CreateOrderItem(3, order2, burger, 1),
                CreateOrderItem(4, order2, pizza, 1),
                CreateOrderItem(5, order3, burger, 1),
                CreateOrderItem(6, order3, pizza, 1),
                CreateOrderItem(7, order3, pasta, 1)
            };

            var repository = CreateFrequentlyBoughtRepository(
                orderItems,
                new List<MenuItem>
                {
                    burger,
                    pizza,
                    pasta
                });

            var result = await repository.GetFrequentlyBoughtTogether(
                1,
                2);

            result.Should().ContainSingle();

            result[0].Item1.Should().Be("Burger");
            result[0].Item2.Should().Be("Pizza");
            result[0].Item3.Should().BeNull();
            result[0].ItemCount.Should().Be(2);
            result[0].NumberOfTimesBought.Should().Be(3);
        }

        [TestMethod]
        [Description("Verifies that only delivered orders are considered when calculating pairs.")]
        public async Task GetFrequentlyBoughtTogether_SizeTwo_IgnoresNonDeliveredOrders()
        {
            var restaurant = CreateRestaurant(
                1,
                10,
                "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");
            var pizza = CreateMenuItem(2, "Pizza");
            var pasta = CreateMenuItem(3, "Pasta");

            var deliveredOrder = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var dispatchedOrder = CreateOrder(
                2,
                1,
                restaurant,
                OrderStatus.Dispatched);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(
                    1,
                    deliveredOrder,
                    burger,
                    1),

                CreateOrderItem(
                    2,
                    deliveredOrder,
                    pizza,
                    1),

                CreateOrderItem(
                    3,
                    dispatchedOrder,
                    burger,
                    1),

                CreateOrderItem(
                    4,
                    dispatchedOrder,
                    pasta,
                    1)
            };

            var repository = CreateFrequentlyBoughtRepository(
                orderItems,
                new List<MenuItem>
                {
                    burger,
                    pizza,
                    pasta
                });

            var result = await repository.GetFrequentlyBoughtTogether(
                1,
                2);

            result.Should().ContainSingle();

            result[0].Item1.Should().Be("Burger");
            result[0].Item2.Should().Be("Pizza");
            result[0].NumberOfTimesBought.Should().Be(1);
        }

        [TestMethod]
        [Description("Verifies that orders from another restaurant are ignored.")]
        public async Task GetFrequentlyBoughtTogether_SizeTwo_FiltersByRestaurant()
        {
            var restaurant1 = CreateRestaurant(
                1,
                101,
                "Restaurant 1");

            var restaurant2 = CreateRestaurant(
                2,
                102,
                "Restaurant 2");

            var burger = CreateMenuItem(1, "Burger", 1);
            var pizza = CreateMenuItem(2, "Pizza", 1);
            var pasta = CreateMenuItem(3, "Pasta", 2);

            var order1 = CreateOrder(
                1,
                1,
                restaurant1,
                OrderStatus.Delivered);

            var order2 = CreateOrder(
                2,
                2,
                restaurant2,
                OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order1, burger, 1),
                CreateOrderItem(2, order1, pizza, 1),

                CreateOrderItem(3, order2, burger, 1),
                CreateOrderItem(4, order2, pasta, 1)
            };

            var repository = CreateFrequentlyBoughtRepository(
                orderItems,
                new List<MenuItem>
                {
                    burger,
                    pizza,
                    pasta
                });

            var result = await repository.GetFrequentlyBoughtTogether(
                1,
                2);

            result.Should().ContainSingle();

            result[0].Item1.Should().Be("Burger");
            result[0].Item2.Should().Be("Pizza");
            result[0].NumberOfTimesBought.Should().Be(1);
        }

        [TestMethod]
        [Description("Verifies that an empty result is returned when no pair exists.")]
        public async Task GetFrequentlyBoughtTogether_SizeTwo_WhenNoPairsExist_ReturnsEmpty()
        {
            var restaurant = CreateRestaurant(
                1,
                101,
                "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");

            var order = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(
                    1,
                    order,
                    burger,
                    1)
            };

            var repository = CreateFrequentlyBoughtRepository(
                orderItems,
                new List<MenuItem>
                {
                    burger
                });

            var result = await repository.GetFrequentlyBoughtTogether(
                1,
                2);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        [Description("Verifies that an empty result is returned when no triplet exists.")]
        public async Task GetFrequentlyBoughtTogether_SizeThree_WhenNoTripletsExist_ReturnsEmpty()
        {
            var restaurant = CreateRestaurant(
                1,
                101,
                "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");
            var pizza = CreateMenuItem(2, "Pizza");

            var order = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order, burger, 1),
                CreateOrderItem(2, order, pizza, 1)
            };

            var repository = CreateFrequentlyBoughtRepository(
                orderItems,
                new List<MenuItem>
                {
                    burger,
                    pizza
                });

            var result = await repository.GetFrequentlyBoughtTogether(
                1,
                3);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        [Description("Verifies that size 2 returns ItemCount as 2.")]
        public async Task GetFrequentlyBoughtTogether_SizeTwo_ShouldSetItemCountToTwo()
        {
            var restaurant = CreateRestaurant(
                1,
                101,
                "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");
            var pizza = CreateMenuItem(2, "Pizza");

            var order = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order, burger, 1),
                CreateOrderItem(2, order, pizza, 1)
            };

            var repository = CreateFrequentlyBoughtRepository(
                orderItems,
                new List<MenuItem>
                {
                    burger,
                    pizza
                });

            var result = await repository.GetFrequentlyBoughtTogether(
                1,
                2);

            result.Should().ContainSingle();
            result[0].ItemCount.Should().Be(2);
            result[0].Item3.Should().BeNull();
        }

        [TestMethod]
        [Description("Verifies that size 3 returns ItemCount as 3.")]
        public async Task GetFrequentlyBoughtTogether_SizeThree_ShouldSetItemCountToThree()
        {
            var restaurant = CreateRestaurant(
                1,
                101,
                "Restaurant 1");

            var burger = CreateMenuItem(1, "Burger");
            var pizza = CreateMenuItem(2, "Pizza");
            var pasta = CreateMenuItem(3, "Pasta");

            var order = CreateOrder(
                1,
                1,
                restaurant,
                OrderStatus.Delivered);

            var orderItems = new List<OrderItem>
            {
                CreateOrderItem(1, order, burger, 1),
                CreateOrderItem(2, order, pizza, 1),
                CreateOrderItem(3, order, pasta, 1)
            };

            var repository = CreateFrequentlyBoughtRepository(
                orderItems,
                new List<MenuItem>
                {
                    burger,
                    pizza,
                    pasta
                });

            var result = await repository.GetFrequentlyBoughtTogether(
                1,
                3);

            result.Should().ContainSingle();
            result[0].ItemCount.Should().Be(3);
            result[0].Item3.Should().Be("Pasta");
        }

        private static ReportRepository CreateRepository(
            List<OrderItem> orderItems)
        {
            var orderItemsDbSet = CreateMockDbSet(orderItems);

            var contextMock = new Mock<RestaurantDbContext>();

            contextMock
                .Setup(x => x.OrderItems)
                .Returns(orderItemsDbSet.Object);

            return new ReportRepository(contextMock.Object);
        }

        private static Mock<DbSet<T>> CreateMockDbSet<T>(
            List<T> data)
            where T : class
        {
            var queryable = data.AsQueryable();

            var mockSet = new Mock<DbSet<T>>();

            mockSet
                .As<IQueryable<T>>()
                .Setup(x => x.Provider)
                .Returns(
                    new TestDbAsyncQueryProvider<T>(
                        queryable.Provider));

            mockSet
                .As<IQueryable<T>>()
                .Setup(x => x.Expression)
                .Returns(queryable.Expression);

            mockSet
                .As<IQueryable<T>>()
                .Setup(x => x.ElementType)
                .Returns(queryable.ElementType);

            mockSet
                .As<IQueryable<T>>()
                .Setup(x => x.GetEnumerator())
                .Returns(
                    () => queryable.GetEnumerator());

            mockSet
                .As<IDbAsyncEnumerable<T>>()
                .Setup(x => x.GetAsyncEnumerator())
                .Returns(
                    new TestDbAsyncEnumerator<T>(
                        queryable.GetEnumerator()));

            return mockSet;
        }

        private static Restaurant CreateRestaurant(
            long id,
            long ownerId,
            string name)
        {
            return new Restaurant
            {
                Id = id,
                OwnerId = ownerId,
                Name = name
            };
        }

        private static MenuItem CreateMenuItem(
            long id,
            string name,
            long restaurantId = 1)
        {
            return new MenuItem
            {
                Id = id,
                Name = name,
                RestaurantId = restaurantId
            };
        }

        private static Order CreateOrder(
            long id,
            long restaurantId,
            Restaurant restaurant,
            OrderStatus status)
        {
            return new Order
            {
                Id = id,
                RestaurantId = restaurantId,
                Restaurant = restaurant,
                Status = status,
                OrderedItems = new List<OrderItem>()
            };
        }

        private static OrderItem CreateOrderItem(
            long id,
            Order order,
            MenuItem menuItem,
            int quantity)
        {
            var orderItem = new OrderItem
            {
                Id = (int)id,
                OrderId = order.Id,
                Order = order,
                MenuItemId = menuItem.Id,
                MenuItem = menuItem,
                Quantity = quantity
            };

            order.OrderedItems.Add(orderItem);

            return orderItem;
        }
    }
}
