using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Tests.Helpers
{
    [TestClass]
    public class PaginationHelperTests
    {
        private static TestAsyncEnumerable<T> CreateAsyncQueryable<T>(IEnumerable<T> data)
        {
            return new TestAsyncEnumerable<T>(data);
        }

        [TestMethod]
        public async Task CreateAsync_ShouldReturnFirstPage()
        {
            var data = new List<int> { 1, 2, 3, 4, 5 };
            var query = CreateAsyncQueryable(data);
            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 2
            };

            var result = await PaginationHelper.CreateAsync(query, request);
            result.Items.Should().BeEquivalentTo(new[] { 1, 2 });
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(2);
            result.TotalCount.Should().Be(5);
            result.TotalPages.Should().Be(3);
            result.HasPreviousPage.Should().BeFalse();
            result.HasNextPage.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_ShouldReturnSecondPage()
        {
            var data = new List<int> { 1, 2, 3, 4, 5 };
            var query = CreateAsyncQueryable(data);
            var request = new PaginationRequest
            {
                Page = 2,
                PageSize = 2
            };

            var result = await PaginationHelper.CreateAsync(query, request);
            result.Items.Should().BeEquivalentTo(new[] { 3, 4 });
            result.Page.Should().Be(2);
            result.PageSize.Should().Be(2);
            result.TotalCount.Should().Be(5);
            result.TotalPages.Should().Be(3);
            result.HasPreviousPage.Should().BeTrue();
            result.HasNextPage.Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAsync_ShouldReturnLastPage()
        {
            var data = new List<int> { 1, 2, 3, 4, 5 };
            var query = CreateAsyncQueryable(data);
            var request = new PaginationRequest
            {
                Page = 3,
                PageSize = 2
            };

            var result = await PaginationHelper.CreateAsync(query, request);
            result.Items.Should().BeEquivalentTo(new[] { 5 });
            result.Page.Should().Be(3);
            result.PageSize.Should().Be(2);
            result.TotalCount.Should().Be(5);
            result.TotalPages.Should().Be(3);
            result.HasPreviousPage.Should().BeTrue();
            result.HasNextPage.Should().BeFalse();
        }

        [TestMethod]
        public async Task CreateAsync_ShouldReturnEmptyResult_WhenNoDataExists()
        {
            var data = new List<int>();
            var query = CreateAsyncQueryable(data);
            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 10
            };

            var result = await PaginationHelper.CreateAsync(query, request);
            result.Items.Should().BeEmpty();
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(0);
            result.TotalPages.Should().Be(0);
            result.HasPreviousPage.Should().BeFalse();
            result.HasNextPage.Should().BeFalse();
        }
    }
}
