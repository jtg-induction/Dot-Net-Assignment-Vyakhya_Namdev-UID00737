using System;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class DashboardOrderRequest : PaginationRequest
    {
        /// Search
        public string Search { get; set; }

        /// Filters
        public long? RestaurantId { get; set; }
        public long? CustomerId { get; set; }
        public long? TotalItems { get; set; }
        public long? MinTotalItems { get; set; }
        public long? MaxTotalItems { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? MinTotalAmount { get; set; }
        public decimal? MaxTotalAmount { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Status { get; set; }

        /// Sorting
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
    }
}
