using DotNetRestaurantManagement.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> GenerateTopOrderedItemsReport(
                long ownerId,
                long? restaurantId,
                IEnumerable<long> excludedItemIds);

        Task<byte[]> GenerateFrequentlyBoughtTogetherReport(
                long ownerId,
                long restaurantId,
                int size);
    }
}
