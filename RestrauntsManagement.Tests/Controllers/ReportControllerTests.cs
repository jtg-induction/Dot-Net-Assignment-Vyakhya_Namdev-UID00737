using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Controllers;
using DotNetRestaurantManagement.Services.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Tests.Controllers
{
    [TestClass]
    public class ReportControllerTests
    {
        private Mock<IReportService> _reportServiceMock;
        private ReportController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            _reportServiceMock = new Mock<IReportService>();
            _controller = new ReportController(_reportServiceMock.Object);
            var identity = new ClaimsIdentity("TestAuth");
            identity.AddClaim(
                new Claim(
                    StringConstants.UserId,
                    "1"));

            var principal = new ClaimsPrincipal(identity);
            _controller.RequestContext.Principal = principal;
            _controller.User = principal;
            Thread.CurrentPrincipal = principal;
            _controller.Request = new HttpRequestMessage();
        }

        [TestMethod]
        [Description("Should return PDF response when top ordered items report is generated successfully")]
        public async Task GetTopOrderedItemsReport_ShouldReturnPdfResponse()
        {
            var reportBytes = new byte[] { 1, 2, 3, 4 };

            _reportServiceMock
                .Setup(x => x.GenerateTopOrderedItemsReport(
                    1,
                    10,
                    It.Is<IEnumerable<long>>(ids =>
                        new List<long>(ids).SequenceEqual(new[] { 2L, 3L }))))
                .ReturnsAsync(reportBytes);

            var response = await _controller.GetTopOrderedItemsReport(
                10,
                new[] { 2L, 3L });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNull();

            var content = await response.Content.ReadAsByteArrayAsync();
            content.Should().Equal(reportBytes);

            response.Content.Headers.ContentType.MediaType
                .Should().Be("application/pdf");

            response.Content.Headers.ContentDisposition
                .Should().NotBeNull();

            response.Content.Headers.ContentDisposition.DispositionType
                .Should().Be("attachment");

            response.Content.Headers.ContentDisposition.FileName
                .Should().Be(StringConstants.TopOrderReportPdfFileName);

            _reportServiceMock.Verify(
                x => x.GenerateTopOrderedItemsReport(
                    1,
                    10,
                    It.Is<IEnumerable<long>>(ids =>
                        new List<long>(ids).SequenceEqual(new[] { 2L, 3L }))),
                Times.Once);
        }

        [TestMethod]
        [Description("Should pass excluded item ids directly to the report service")]
        public async Task GetTopOrderedItemsReport_ShouldPassExcludedItemIdsToService()
        {
            var reportBytes = new byte[] { 10, 20 };

            _reportServiceMock
                .Setup(x => x.GenerateTopOrderedItemsReport(
                    1,
                    null,
                    It.Is<IEnumerable<long>>(ids =>
                        new List<long>(ids).SequenceEqual(
                            new[] { 5L, 8L, 10L }))))
                .ReturnsAsync(reportBytes);

            var response = await _controller.GetTopOrderedItemsReport(
                null,
                new[] { 5L, 8L, 10L });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            _reportServiceMock.Verify(
                x => x.GenerateTopOrderedItemsReport(
                    1,
                    null,
                    It.Is<IEnumerable<long>>(ids =>
                        new List<long>(ids).SequenceEqual(
                            new[] { 5L, 8L, 10L }))),
                Times.Once);
        }

        [TestMethod]
        [Description("Should propagate service exception when report generation fails")]
        public async Task GetTopOrderedItemsReport_WhenServiceThrows_ShouldPropagateException()
        {
            var exception = new InvalidOperationException("Report generation failed");

            _reportServiceMock
                .Setup(x => x.GenerateTopOrderedItemsReport(
                    It.IsAny<long>(),
                    It.IsAny<long?>(),
                    It.IsAny<IEnumerable<long>>()))
                .ThrowsAsync(exception);

            Func<Task> action = () =>
                _controller.GetTopOrderedItemsReport();

            await action.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Report generation failed");
        }

        [TestMethod]
        [Description("Should pass excluded item ids directly without manual string parsing")]
        public async Task GetTopOrderedItemsReport_ShouldPassExcludedItemIdsDirectly()
        {
            var reportBytes = new byte[] { 1, 2, 3 };
            var excludedItemIds = new[] { 1L, 3L, 5L };

            _reportServiceMock
                .Setup(x => x.GenerateTopOrderedItemsReport(
                    1,
                    13,
                    It.Is<IEnumerable<long>>(ids =>
                        ids.SequenceEqual(excludedItemIds))))
                .ReturnsAsync(reportBytes);

            var response = await _controller.GetTopOrderedItemsReport(
                13,
                excludedItemIds);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            _reportServiceMock.Verify(
                x => x.GenerateTopOrderedItemsReport(
                    1,
                    13,
                    It.Is<IEnumerable<long>>(ids =>
                        ids.SequenceEqual(excludedItemIds))),
                Times.Once);
        }

        [TestMethod]
        [Description("Should return PDF response when frequently bought together report is generated successfully")]
        public async Task GetFrequentlyBoughtTogetherReport_ShouldReturnPdfResponse()
        {
            var restaurantId = 13L;
            var combinationSize = 2;
            var limit = 10;
            var reportBytes = new byte[] { 1, 2, 3, 4 };

            _reportServiceMock
                .Setup(x => x.GenerateFrequentlyBoughtTogetherReport(
                    1,
                    restaurantId,
                    combinationSize,
                    limit))
                .ReturnsAsync(reportBytes);

            var response = await _controller.GetFrequentlyBoughtTogetherReport(
                restaurantId,
                combinationSize,
                limit);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Should().NotBeNull();

            var content = await response.Content.ReadAsByteArrayAsync();

            content.Should().Equal(reportBytes);

            response.Content.Headers.ContentType
                .Should().NotBeNull();

            response.Content.Headers.ContentType.MediaType
                .Should().Be("application/pdf");

            response.Content.Headers.ContentDisposition
                .Should().NotBeNull();

            response.Content.Headers.ContentDisposition.DispositionType
                .Should().Be("attachment");

            response.Content.Headers.ContentDisposition.FileName
                .Should().Be(
                    StringConstants.FrequentlyBoughtItemsPdfFileName);

            _reportServiceMock.Verify(
                x => x.GenerateFrequentlyBoughtTogetherReport(
                    1,
                    restaurantId,
                    combinationSize,
                    limit),
                Times.Once);
        }


        [TestMethod]
        [Description("Should pass restaurant id, combination size and top N correctly to the report service")]
        public async Task GetFrequentlyBoughtTogetherReport_ShouldPassCorrectParametersToService()
        {
            var restaurantId = 25L;
            var combinationSize = 2;
            var limit = 5;
            var reportBytes = new byte[] { 10, 20, 30 };

            _reportServiceMock
                .Setup(x => x.GenerateFrequentlyBoughtTogetherReport(
                    1,
                    restaurantId,
                    combinationSize,
                    limit))
                .ReturnsAsync(reportBytes);

            var response = await _controller.GetFrequentlyBoughtTogetherReport(
                restaurantId,
                combinationSize,
                limit);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            _reportServiceMock.Verify(
                x => x.GenerateFrequentlyBoughtTogetherReport(
                    1,
                    restaurantId,
                    combinationSize,
                    limit),
                Times.Once);
        }

        [TestMethod]
        [Description("Should return the exact report bytes generated by the service")]
        public async Task GetFrequentlyBoughtTogetherReport_ShouldReturnGeneratedReportBytes()
        {
            var restaurantId = 13L;
            var combinationSize = 2;
            var limit = 10;
            var expectedReport = new byte[] { 100, 101, 102, 103 };

            _reportServiceMock
                .Setup(x => x.GenerateFrequentlyBoughtTogetherReport(
                    1,
                    restaurantId,
                    combinationSize,
                    limit))
                .ReturnsAsync(expectedReport);

            var response = await _controller.GetFrequentlyBoughtTogetherReport(
                restaurantId,
                combinationSize,
                limit);

            var actualReport = await response.Content.ReadAsByteArrayAsync();

            actualReport.Should().BeEquivalentTo(expectedReport);
        }


        [TestMethod]
        [Description("Should propagate service exception when frequently bought together report generation fails")]
        public async Task GetFrequentlyBoughtTogetherReport_WhenServiceThrows_ShouldPropagateException()
        {
            var restaurantId = 13L;
            var combinationSize = 2;
            var limit = 10;

            var exception = new InvalidOperationException(
                "Report generation failed");

            _reportServiceMock
                .Setup(x => x.GenerateFrequentlyBoughtTogetherReport(
                    1,
                    restaurantId,
                    combinationSize,
                    limit))
                .ThrowsAsync(exception);

            Func<Task> action = () =>
                _controller.GetFrequentlyBoughtTogetherReport(
                    restaurantId,
                    combinationSize,
                    limit);

            await action.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Report generation failed");

            _reportServiceMock.Verify(
                x => x.GenerateFrequentlyBoughtTogetherReport(
                    1,
                    restaurantId,
                    combinationSize,
                    limit),
                Times.Once);
        }


        [TestMethod]
        [Description("Should use the owner id from the authenticated user when generating the report")]
        public async Task GetFrequentlyBoughtTogetherReport_ShouldUseAuthenticatedOwnerId()
        {
            var restaurantId = 50L;
            var combinationSize = 2;
            var limit = 5;
            var reportBytes = new byte[] { 5, 6, 7 };

            _reportServiceMock
                .Setup(x => x.GenerateFrequentlyBoughtTogetherReport(
                    1,
                    restaurantId,
                    combinationSize,
                    limit))
                .ReturnsAsync(reportBytes);

            await _controller.GetFrequentlyBoughtTogetherReport(
                restaurantId,
                combinationSize,
                limit);

            _reportServiceMock.Verify(
                x => x.GenerateFrequentlyBoughtTogetherReport(
                    1,
                    restaurantId,
                    combinationSize,
                    limit),
                Times.Once);
        }
    }
}
