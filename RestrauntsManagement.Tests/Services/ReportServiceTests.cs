using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Repositories;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services;
using DotNetRestaurantManagement.Services.Implementations;
using DotNetRestaurantManagement.Services.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Tests.Services
{
    [TestClass]
    public class ReportServiceTests
    {
        private Mock<IReportRepository> _reportRepositoryMock;
        private Mock<IGenerateReportService> _generateReportServiceMock;
        private ReportService _reportService;

        [TestInitialize]
        public void TestInitialize()
        {
            _reportRepositoryMock = new Mock<IReportRepository>();
            _generateReportServiceMock = new Mock<IGenerateReportService>();

            _reportService = new ReportService(
                _reportRepositoryMock.Object,
                _generateReportServiceMock.Object);
        }

        [TestMethod]
        [Description("Should generate the report when the restaurant belongs to the owner.")]
        public async Task GenerateTopOrderedItemsReport_WhenRestaurantBelongsToOwner_ShouldReturnReport()
        {
            var ownerId = 1L;
            var restaurantId = 10L;
            var excludedItemIds = new List<long> { 2L, 3L };

            var reportData = new List<Top10OrderedItemsReport>
            {
                new Top10OrderedItemsReport
                {
                    RestaurantId = restaurantId,
                    RestaurantName = "Restaurant 1",
                    MenuItemId = 100,
                    MenuItemName = "Burger",
                    NumberOfTimesOrdered = 10
                }
            };

            var expectedReport = new byte[] { 1, 2, 3 };

            _reportRepositoryMock
                .Setup(x => x.CheckRestaurantBelongsToOwner(
                    restaurantId,
                    ownerId))
                .ReturnsAsync(true);

            _reportRepositoryMock
                .Setup(x => x.GetTopOrderedItems(
                    ownerId,
                    restaurantId,
                    excludedItemIds))
                .ReturnsAsync(reportData);

            _generateReportServiceMock
                .Setup(x => x.Generate(
                    reportData,
                    StringConstants.TopOrderReportFileName,
                    false,
                    null))
                .Returns(expectedReport);

            var result = await _reportService.GenerateTopOrderedItemsReport(
                ownerId,
                restaurantId,
                excludedItemIds);

            result.Should().Equal(expectedReport);

            _reportRepositoryMock.Verify(
                x => x.CheckRestaurantBelongsToOwner(
                    restaurantId,
                    ownerId),
                Times.Once);

            _reportRepositoryMock.Verify(
                x => x.GetTopOrderedItems(
                    ownerId,
                    restaurantId,
                    excludedItemIds),
                Times.Once);

            _generateReportServiceMock.Verify(
                x => x.Generate(
                    reportData,
                    StringConstants.TopOrderReportFileName,
                    false,
                    null),
                Times.Once);
        }

        [TestMethod]
        [Description("Should skip restaurant ownership validation when restaurant id is not provided.")]
        public async Task GenerateTopOrderedItemsReport_WhenRestaurantIdIsNull_ShouldSkipOwnershipCheck()
        {
            var ownerId = 1L;
            long? restaurantId = null;
            var excludedItemIds = new List<long>();

            var reportData = new List<Top10OrderedItemsReport>();
            var expectedReport = new byte[] { 4, 5, 6 };

            _reportRepositoryMock
                .Setup(x => x.GetTopOrderedItems(
                    ownerId,
                    null,
                    excludedItemIds))
                .ReturnsAsync(reportData);

            _generateReportServiceMock
                .Setup(x => x.Generate(
                    reportData,
                    StringConstants.TopOrderReportFileName,
                    true,
                    null))
                .Returns(expectedReport);

            var result = await _reportService.GenerateTopOrderedItemsReport(
                ownerId,
                restaurantId,
                excludedItemIds);

            result.Should().Equal(expectedReport);

            _reportRepositoryMock.Verify(
                x => x.CheckRestaurantBelongsToOwner(
                    It.IsAny<long?>(),
                    It.IsAny<long>()),
                Times.Never);

            _reportRepositoryMock.Verify(
                x => x.GetTopOrderedItems(
                    ownerId,
                    null,
                    excludedItemIds),
                Times.Once);

            _generateReportServiceMock.Verify(
                x => x.Generate(
                    reportData,
                    StringConstants.TopOrderReportFileName,
                    true,
                    null),
                Times.Once);
        }

        [TestMethod]
        [Description("Should pass false for showRestaurantData when restaurant id is provided.")]
        public async Task GenerateTopOrderedItemsReport_WhenRestaurantIdIsProvided_ShouldPassFalseToGenerator()
        {
            var ownerId = 1L;
            var restaurantId = 10L;
            var excludedItemIds = new List<long>();

            var reportData = new List<Top10OrderedItemsReport>();
            var expectedReport = new byte[] { 7, 8, 9 };

            _reportRepositoryMock
                .Setup(x => x.CheckRestaurantBelongsToOwner(
                    restaurantId,
                    ownerId))
                .ReturnsAsync(true);

            _reportRepositoryMock
                .Setup(x => x.GetTopOrderedItems(
                    ownerId,
                    restaurantId,
                    excludedItemIds))
                .ReturnsAsync(reportData);

            _generateReportServiceMock
                .Setup(x => x.Generate(
                    reportData,
                    StringConstants.TopOrderReportFileName,
                    false,
                    null))
                .Returns(expectedReport);

            var result = await _reportService.GenerateTopOrderedItemsReport(
                ownerId,
                restaurantId,
                excludedItemIds);

            result.Should().Equal(expectedReport);

            _generateReportServiceMock.Verify(
                x => x.Generate(
                    reportData,
                    StringConstants.TopOrderReportFileName,
                    false,
                    null),
                Times.Once);
        }

        [TestMethod]
        [Description("Should pass true for showRestaurantData when restaurant id is not provided.")]
        public async Task GenerateTopOrderedItemsReport_WhenRestaurantIdIsNull_ShouldPassTrueToGenerator()
        {
            var ownerId = 1L;
            long? restaurantId = null;
            var excludedItemIds = new List<long>();

            var reportData = new List<Top10OrderedItemsReport>();
            var expectedReport = new byte[] { 10, 11, 12 };

            _reportRepositoryMock
                .Setup(x => x.GetTopOrderedItems(
                    ownerId,
                    null,
                    excludedItemIds))
                .ReturnsAsync(reportData);

            _generateReportServiceMock
                .Setup(x => x.Generate(
                    reportData,
                    StringConstants.TopOrderReportFileName,
                    true,
                    null))
                .Returns(expectedReport);

            var result = await _reportService.GenerateTopOrderedItemsReport(
                ownerId,
                restaurantId,
                excludedItemIds);

            result.Should().Equal(expectedReport);

            _generateReportServiceMock.Verify(
                x => x.Generate(
                    reportData,
                    StringConstants.TopOrderReportFileName,
                    true,
                    null),
                Times.Once);
        }

        [TestMethod]
        [Description("Should pass excluded item ids to the repository.")]
        public async Task GenerateTopOrderedItemsReport_ShouldPassExcludedItemIdsToRepository()
        {
            var ownerId = 1L;
            long? restaurantId = null;
            var excludedItemIds = new List<long> { 10L, 20L, 30L };

            var reportData = new List<Top10OrderedItemsReport>();
            var expectedReport = new byte[] { 1, 2 };

            _reportRepositoryMock
                .Setup(x => x.GetTopOrderedItems(
                    ownerId,
                    restaurantId,
                    excludedItemIds))
                .ReturnsAsync(reportData);

            _generateReportServiceMock
                .Setup(x => x.Generate(
                    reportData,
                    StringConstants.TopOrderReportFileName,
                    true,
                    null))
                .Returns(expectedReport);

            await _reportService.GenerateTopOrderedItemsReport(
                ownerId,
                restaurantId,
                excludedItemIds);

            _reportRepositoryMock.Verify(
                x => x.GetTopOrderedItems(
                    ownerId,
                    restaurantId,
                    excludedItemIds),
                Times.Once);
        }

        [TestMethod]
        [Description("Should return the report generated by the report generator service.")]
        public async Task GenerateTopOrderedItemsReport_ShouldReturnGeneratedReport()
        {
            var ownerId = 1L;
            long? restaurantId = null;
            var excludedItemIds = new List<long>();

            var reportData = new List<Top10OrderedItemsReport>();
            var expectedReport = new byte[] { 100, 101, 102 };

            _reportRepositoryMock
                .Setup(x => x.GetTopOrderedItems(
                    ownerId,
                    restaurantId,
                    excludedItemIds))
                .ReturnsAsync(reportData);

            _generateReportServiceMock
                .Setup(x => x.Generate(
                    reportData,
                    StringConstants.TopOrderReportFileName,
                    true,
                    null))
                .Returns(expectedReport);

            var result = await _reportService.GenerateTopOrderedItemsReport(
                ownerId,
                restaurantId,
                excludedItemIds);

            result.Should().BeSameAs(expectedReport);
        }

        [TestMethod]
        [Description("Should throw bad request when the restaurant does not belong to the owner.")]
        public async Task GenerateFrequentlyBoughtTogetherReport_WhenRestaurantDoesNotBelongToOwner_ShouldThrowBadRequest()
        {
            var ownerId = 1L;
            var restaurantId = 10L;
            var size = 2;

            _reportRepositoryMock
                .Setup(x => x.CheckRestaurantBelongsToOwner(
                    restaurantId,
                    ownerId))
                .ReturnsAsync(false);

            Func<Task> action = async () =>
                await _reportService.GenerateFrequentlyBoughtTogetherReport(
                    ownerId,
                    restaurantId,
                    size);

            var exception = await action.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.BadRequest);

            exception.Which.Message
                .Should().Be(
                    ErrorMessages.RestaurantDoesNotBelongsToUser);

            _reportRepositoryMock.Verify(
                x => x.CheckRestaurantBelongsToOwner(
                    restaurantId,
                    ownerId),
                Times.Once);

            _reportRepositoryMock.Verify(
                x => x.GetFrequentlyBoughtTogether(
                    It.IsAny<long>(),
                    It.IsAny<int>()),
                Times.Never);

            _generateReportServiceMock.Verify(
                x => x.Generate(
                    It.IsAny<IEnumerable<FrequentlyBoughtItems>>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int?>()),
                Times.Never);
        }

        [TestMethod]
        [Description("Should throw bad request when the frequently bought together size is invalid.")]
        public async Task GenerateFrequentlyBoughtTogetherReport_WhenSizeIsInvalid_ShouldThrowBadRequest()
        {
            var ownerId = 1L;
            var restaurantId = 10L;
            var size = 4;

            _reportRepositoryMock
                .Setup(x => x.CheckRestaurantBelongsToOwner(
                    restaurantId,
                    ownerId))
                .ReturnsAsync(true);

            Func<Task> action = async () =>
                await _reportService.GenerateFrequentlyBoughtTogetherReport(
                    ownerId,
                    restaurantId,
                    size);

            var exception = await action.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.BadRequest);

            exception.Which.Message
                .Should().Be(
                    ErrorMessages.FrequentlyBoughtTogetherSizeError);

            _reportRepositoryMock.Verify(
                x => x.CheckRestaurantBelongsToOwner(
                    restaurantId,
                    ownerId),
                Times.Once);

            _reportRepositoryMock.Verify(
                x => x.GetFrequentlyBoughtTogether(
                    It.IsAny<long>(),
                    It.IsAny<int>()),
                Times.Never);

            _generateReportServiceMock.Verify(
                x => x.Generate(
                    It.IsAny<IEnumerable<FrequentlyBoughtItems>>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int?>()),
                Times.Never);
        }

        [TestMethod]
        [Description("Should pass the restaurant id and size to the repository.")]
        public async Task GenerateFrequentlyBoughtTogetherReport_ShouldPassRestaurantIdAndSizeToRepository()
        {
            var ownerId = 5L;
            var restaurantId = 25L;
            var size = 2;

            var reportData = new List<FrequentlyBoughtItems>
    {
        new FrequentlyBoughtItems
        {
            Item1 = "Burger",
            Item2 = "Pizza",
            ItemCount = 2,
            NumberOfTimesBought = 3
        }
    };

            var expectedReport = new byte[] { 10, 20 };

            _reportRepositoryMock
                .Setup(x => x.CheckRestaurantBelongsToOwner(
                    restaurantId,
                    ownerId))
                .ReturnsAsync(true);

            _reportRepositoryMock
                .Setup(x => x.GetFrequentlyBoughtTogether(
                    restaurantId,
                    size))
                .ReturnsAsync(reportData);

            _generateReportServiceMock
                .Setup(x => x.Generate(
                    reportData,
                    StringConstants.FrequentlyBoughtItemsReportFileName,
                    null,
                    size))
                .Returns(expectedReport);

            await _reportService.GenerateFrequentlyBoughtTogetherReport(
                ownerId,
                restaurantId,
                size);

            _reportRepositoryMock.Verify(
                x => x.GetFrequentlyBoughtTogether(
                    restaurantId,
                    size),
                Times.Once);
        }

        [TestMethod]
        [Description("Should pass item count 2 to the report generator for size 2.")]
        public async Task GenerateFrequentlyBoughtTogetherReport_WhenSizeIsTwo_ShouldPassItemCountTwo()
        {
            var ownerId = 1L;
            var restaurantId = 10L;
            var size = 2;

            var reportData = new List<FrequentlyBoughtItems>
                                    {
                                        new FrequentlyBoughtItems
                                        {
                                            Item1 = "Burger",
                                            Item2 = "Pizza",
                                            ItemCount = 2,
                                            NumberOfTimesBought = 5
                                        }
                                    };

            var expectedReport = new byte[] { 1, 2, 3 };

            _reportRepositoryMock
                .Setup(x => x.CheckRestaurantBelongsToOwner(
                    restaurantId,
                    ownerId))
                .ReturnsAsync(true);

            _reportRepositoryMock
                .Setup(x => x.GetFrequentlyBoughtTogether(
                    restaurantId,
                    size))
                .ReturnsAsync(reportData);

            _generateReportServiceMock
                .Setup(x => x.Generate(
                    reportData,
                    StringConstants.FrequentlyBoughtItemsReportFileName,
                    null,
                    2))
                .Returns(expectedReport);

            await _reportService.GenerateFrequentlyBoughtTogetherReport(
                ownerId,
                restaurantId,
                size);

            _generateReportServiceMock.Verify(
                x => x.Generate(
                    reportData,
                    StringConstants.FrequentlyBoughtItemsReportFileName,
                    null,
                    2),
                Times.Once);
        }

        [TestMethod]
        [Description("Should pass item count 3 to the report generator for size 3.")]
        public async Task GenerateFrequentlyBoughtTogetherReport_WhenSizeIsThree_ShouldPassItemCountThree()
        {
            var ownerId = 1L;
            var restaurantId = 10L;
            var size = 3;

            var reportData = new List<FrequentlyBoughtItems>
                                {
                                    new FrequentlyBoughtItems
                                    {
                                        Item1 = "Burger",
                                        Item2 = "Pizza",
                                        Item3 = "Pasta",
                                        ItemCount = 3,
                                        NumberOfTimesBought = 5
                                    }
                                };

            var expectedReport = new byte[] { 4, 5, 6 };
            _reportRepositoryMock
                .Setup(x => x.CheckRestaurantBelongsToOwner(
                    restaurantId,
                    ownerId))
                .ReturnsAsync(true);

            _reportRepositoryMock
                .Setup(x => x.GetFrequentlyBoughtTogether(
                    restaurantId,
                    size))
                .ReturnsAsync(reportData);

            _generateReportServiceMock
                .Setup(x => x.Generate(
                    reportData,
                    StringConstants.FrequentlyBoughtItemsReportFileName,
                    null,
                    3))
                .Returns(expectedReport);

            await _reportService.GenerateFrequentlyBoughtTogetherReport(
                ownerId,
                restaurantId,
                size);

            _generateReportServiceMock.Verify(
                x => x.Generate(
                    reportData,
                    StringConstants.FrequentlyBoughtItemsReportFileName,
                    null,
                    3),
                Times.Once);
        }
    }
}
