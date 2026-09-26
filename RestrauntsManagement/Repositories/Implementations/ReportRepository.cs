using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Implementations
{
    public class ReportRepository : IReportRepository
    {
        private readonly RestaurantDbContext _context;
        public ReportRepository(RestaurantDbContext context)
        {
            _context = context;
        }
        public async Task<List<Top10OrderedItemsReport>> GetTopOrderedItems(
            long ownerId, long? restaurantId,
            IEnumerable<long> excludedItemIds)
        {
            var excludedIds = excludedItemIds == null
                ? new List<long>()
                : new List<long>(excludedItemIds);

            var topItems = _context.OrderItems
                .Where(orderedItem =>
                orderedItem.Order.Restaurant.OwnerId == ownerId
                && orderedItem.Order.Status == OrderStatus.Delivered);

            if (restaurantId.HasValue)
            {
                topItems = topItems.Where(orderedItem =>
                        orderedItem.Order.RestaurantId == restaurantId);
            }

            if (excludedIds.Any())
            {
                topItems = topItems.Where(orderedItem =>
                        !excludedIds.Contains(orderedItem.MenuItemId));
            }

            return await topItems
                .GroupBy(orderedItem => new
                {
                    orderedItem.Order.RestaurantId,
                    RestaurantName = orderedItem.Order.Restaurant.Name,
                    orderedItem.MenuItemId,
                    MenuItemName = orderedItem.MenuItem.Name
                })
                .Select(group => new Top10OrderedItemsReport
                {
                    RestaurantId = group.Key.RestaurantId,
                    RestaurantName = group.Key.RestaurantName,
                    MenuItemId = group.Key.MenuItemId,
                    MenuItemName = group.Key.MenuItemName,
                    NumberOfTimesOrdered = group.Sum(item => item.Quantity)
                })
                .OrderByDescending(item => item.NumberOfTimesOrdered)
                .Take(10)
                .ToListAsync();
        }

        public async Task<List<FrequentlyBoughtItems>> GetFrequentlyBoughtTogether(
            long restaurantId,
            int size)
        {
            if (size == 2)
            {
                return await GetFrequentlyBoughtPairsAsync(restaurantId);
            }

            return await GetFrequentlyBoughtTripletsAsync(restaurantId);
        }

        private async Task<List<FrequentlyBoughtItems>> GetFrequentlyBoughtPairsAsync(
                long restaurantId)
        {
            var pairCounts = await _context.OrderItems
                .Where(x =>
                    x.Order.RestaurantId == restaurantId &&
                    x.Order.Status == OrderStatus.Delivered)
                .SelectMany(
                    item1 => item1.Order.OrderedItems,
                    (item1, item2) => new
                    {
                        Item1 = item1,
                        Item2 = item2
                    })
                .Where(x =>
                    x.Item1.MenuItemId < x.Item2.MenuItemId)
                .GroupBy(x => new
                {
                    Item1Id = x.Item1.MenuItemId,
                    Item2Id = x.Item2.MenuItemId,
                    Item1Name = x.Item1.MenuItem.Name,
                    Item2Name = x.Item2.MenuItem.Name
                })
                .Select(group => new
                {
                    group.Key.Item1Id,
                    group.Key.Item2Id,
                    group.Key.Item1Name,
                    group.Key.Item2Name,
                    NumberOfTimesOrdered = group
                        .Select(x => x.Item1.OrderId)
                        .Distinct()
                        .Count()
                })
                .ToListAsync();

            if (!pairCounts.Any())
            {
                return new List<FrequentlyBoughtItems>();
            }

            var maxCount = pairCounts.Max(x => x.NumberOfTimesOrdered);
            return pairCounts
                .Where(x => x.NumberOfTimesOrdered == maxCount)
                .Select(x => new FrequentlyBoughtItems
                {
                    Item1 = x.Item1Name,
                    Item2 = x.Item2Name,
                    ItemCount = 2,
                    NumberOfTimesBought = x.NumberOfTimesOrdered
                })
                .ToList();
        }

        private async Task<List<FrequentlyBoughtItems>> GetFrequentlyBoughtTripletsAsync(
            long restaurantId)
        {
            var tripletCounts = await _context.OrderItems
                .Where(x =>
                    x.Order.RestaurantId == restaurantId &&
                    x.Order.Status == OrderStatus.Delivered)
                .SelectMany(
                    item1 => item1.Order.OrderedItems,
                    (item1, item2) => new
                    {
                        Item1 = item1,
                        Item2 = item2
                    })
                .SelectMany(
                    x => x.Item1.Order.OrderedItems,
                    (x, item3) => new
                    {
                        Item1 = x.Item1,
                        Item2 = x.Item2,
                        Item3 = item3
                    })
                .Where(x =>
                    x.Item1.MenuItemId < x.Item2.MenuItemId &&
                    x.Item2.MenuItemId < x.Item3.MenuItemId)
                .GroupBy(x => new
                {
                    Item1Id = x.Item1.MenuItemId,
                    Item2Id = x.Item2.MenuItemId,
                    Item3Id = x.Item3.MenuItemId,
                    Item1Name = x.Item1.MenuItem.Name,
                    Item2Name = x.Item2.MenuItem.Name,
                    Item3Name = x.Item3.MenuItem.Name
                })
                .Select(group => new
                {
                    group.Key.Item1Id,
                    group.Key.Item2Id,
                    group.Key.Item3Id,
                    group.Key.Item1Name,
                    group.Key.Item2Name,
                    group.Key.Item3Name,
                    NumberOfTimesOrdered = group
                        .Select(x => x.Item1.OrderId)
                        .Distinct()
                        .Count()
                })
                .ToListAsync();

            if (!tripletCounts.Any())
            {
                return new List<FrequentlyBoughtItems>();
            }

            var maxCount = tripletCounts.Max(x => x.NumberOfTimesOrdered);
            return tripletCounts
                .Where(x => x.NumberOfTimesOrdered == maxCount)
                .Select(x => new FrequentlyBoughtItems
                {
                    Item1 = x.Item1Name,
                    Item2 = x.Item2Name,
                    Item3 = x.Item3Name,
                    ItemCount = 3,
                    NumberOfTimesBought = x.NumberOfTimesOrdered
                })
                .ToList();
        }

        public async Task<bool> CheckRestaurantBelongsToOwner(long? restaurantId, long ownerId)
        {
            return await _context.Restaurants
                .AnyAsync(x => x.Id == restaurantId
                       && x.OwnerId == ownerId);
        }
    }
}
