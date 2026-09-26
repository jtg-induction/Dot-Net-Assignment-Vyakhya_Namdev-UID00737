using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<List<Top10OrderedItemsReport>> GetTopOrderedItems(
            long ownerId,
            long? restaurantId,
            IEnumerable<long> excludedItemIds);

        Task<List<FrequentlyBoughtItems>> GetFrequentlyBoughtTogether(
            long restaurantId,
            int size);
        Task<bool> CheckRestaurantBelongsToOwner(long? restaurantId, long ownerId);
    }
}
