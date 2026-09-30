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

        /// <summary>
        /// Generates and downloads a PDF report of the most ordered items
        /// </summary>
        [JwtAuthorize(UserRole.Owner, UserRole.SuperAdmin)]
        [HttpGet]
        [Route("top-ordered-items")]
        public async Task<HttpResponseMessage> GetTopOrderedItemsReport(
            [FromUri] long? restaurantId = null,
            [FromUri] IEnumerable<long> excludeItemIds = null)
        {
            var isSuperAdmin = User.IsInRole(StringConstants.SuperAdmin);
            long? ownerId = isSuperAdmin
                ? (long?)null
                : ClaimsHelper.GetUserId(User);
            var excludedItemIds = excludeItemIds;
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

        /// <summary>
        /// Generates and downloads a PDF report showing items that are frequently bought together
        /// </summary>
        [JwtAuthorize(UserRole.Owner, UserRole.SuperAdmin)]
        [HttpGet]
        [Route("restaurants/{restaurantId}/frequently-bought-together")]
        public async Task<HttpResponseMessage> GetFrequentlyBoughtTogetherReport(
            long restaurantId,
            int combinationSize,
            int limit)
        {
            var isSuperAdmin = User.IsInRole(StringConstants.SuperAdmin);
            long? ownerId = isSuperAdmin
                ? (long?)null
                : ClaimsHelper.GetUserId(User);
            var report = await _reportService.GenerateFrequentlyBoughtTogetherReport(
                ownerId,
                restaurantId,
                combinationSize, limit);

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
