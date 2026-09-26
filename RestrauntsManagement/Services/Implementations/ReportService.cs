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

        public async Task<byte[]> GenerateTopOrderedItemsReport(
                long ownerId,
                long? restaurantId,
                IEnumerable<long> excludedItemIds)
        {
            if (restaurantId != null)
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

        public async Task<byte[]> GenerateFrequentlyBoughtTogetherReport(
                long ownerId,
                long restaurantId,
                int size)
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

            if (size != 2 && size != 3)
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    ErrorMessages.FrequentlyBoughtTogetherSizeError);
            }

            var reportData = await _reportRepository.GetFrequentlyBoughtTogether(
                                                restaurantId,
                                                size);

            if (!reportData.Any())
            {
                throw new ApiException(
                    HttpStatusCode.NotFound,
                    ErrorMessages.FrequentlyBoughtItemsNotFound);
            }

            return _generateReportService.Generate(
                    reportData,
                    StringConstants.FrequentlyBoughtItemsReportFileName,
                    itemCount: size);
        }
    }
}
