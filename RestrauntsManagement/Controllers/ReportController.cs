using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Filters;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace DotNetRestaurantManagement.Controllers
{
    [RoutePrefix("api/reports")]
    public class ReportController : ApiController
    {
        private readonly IReportService _reportService;
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        private IEnumerable<long> ParseExcludedItemIds(string excludedItemIds)
        {
            if (string.IsNullOrWhiteSpace(excludedItemIds))
            {
                return Enumerable.Empty<long>();
            }
            return excludedItemIds
                   .Split(',')
                   .Select(long.Parse)
                   .Distinct()
                   .ToList();
        }

        [JwtAuthorize]
        [RoleAuthorize(UserRole.Owner)]
        [HttpGet]
        [Route("top-ordered-items")]
        public async Task<HttpResponseMessage> GetTopOrderedItemsReport(
            long? restaurantId = null,
            string excludeItemIds = null)
        {
            var ownerId = ClaimsHelper.GetUserId(User);
            var excludedItemIds = ParseExcludedItemIds(excludeItemIds);
            var report = await _reportService.GenerateTopOrderedItemsReport(
                ownerId,
                restaurantId,
                excludedItemIds);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(report);
            response.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/pdf");

            response.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = StringConstants.TopOrderReportPdfFileName
                };

            return response;
        }

        [JwtAuthorize]
        [RoleAuthorize(UserRole.Owner)]
        [HttpGet]
        [Route("{restaurantId}/frequently-bought-together/{numberOfItems}")]
        public async Task<HttpResponseMessage> GetFrequentlyBoughtTogetherReport(
            long restaurantId,
            int numberOfItems)
        {
            var ownerId = ClaimsHelper.GetUserId(User);
            var report = await _reportService.GenerateFrequentlyBoughtTogetherReport(
                ownerId,
                restaurantId,
                numberOfItems);

            var response = Request.CreateResponse(HttpStatusCode.OK);

            response.Content = new ByteArrayContent(report);
            response.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/pdf");

            response.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = StringConstants.FrequentlyBoughtItemsPdfFileName
                };

            return response;
        }
    }
}
