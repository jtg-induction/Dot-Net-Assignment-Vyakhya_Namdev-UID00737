using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IGenerateReportService _generateReportService;
        public ReportService(IReportRepository reportRepository, IGenerateReportService generateReportService)
        {
            _reportRepository = reportRepository;
            _generateReportService = generateReportService;
        }

        /// <summary>
        /// Generates a report containing the top ordered items for the requested restaurant or owner
        /// </summary>
        public async Task<byte[]> GenerateTopOrderedItemsReport(
                long? ownerId,
                long? restaurantId,
                IEnumerable<long> excludedItemIds)
        {
            if (ownerId.HasValue && restaurantId.HasValue)
            {
                var owner = await _reportRepository
                                    .CheckRestaurantBelongsToOwner(restaurantId, ownerId);
                if (!owner)
                {
                    throw new ApiException(HttpStatusCode.BadRequest,
                                           ErrorMessages.RestaurantDoesNotBelongsToUser);
                }
            }
            var reportData = await _reportRepository.GetTopOrderedItems(
                ownerId,
                restaurantId,
                excludedItemIds);

            var showRestaurantData = !restaurantId.HasValue;
            return _generateReportService.Generate(
                                            reportData,
                                            StringConstants.TopOrderReportFileName,
                                            showRestaurantData);
        }

        /// <summary>
        /// Generates a report showing items that are frequently bought together for a restaurant
        /// </summary>
        public async Task<byte[]> GenerateFrequentlyBoughtTogetherReport(
                long? ownerId,
                long restaurantId,
                int combinationSize,
                int limit)
        {
            if (ownerId.HasValue)
            {
                var owner = await _reportRepository.CheckRestaurantBelongsToOwner(
                                                restaurantId,
                                                ownerId);
                if (!owner)
                {
                    throw new ApiException(
                        HttpStatusCode.BadRequest,
                        ErrorMessages.RestaurantDoesNotBelongsToUser);
                }
            }

            if (combinationSize != 2 && combinationSize != 3)
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    ErrorMessages.FrequentlyBoughtTogetherSizeError);
            }

            if (limit <= 0)
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    ErrorMessages.InvalidLimit);
            }

            var reportData = await _reportRepository.GetFrequentlyBoughtTogether(
                                                restaurantId,
                                                combinationSize, limit);

            return _generateReportService.GenerateFrequentlyBoughtTogether(
                    reportData,
                    combinationSize);
        }
    }
}
