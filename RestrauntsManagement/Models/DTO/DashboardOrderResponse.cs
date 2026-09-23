using System;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class DashboardOrderResponse
    {
        public long OrderId { get; set; }
        public long RestaurantId { get; set; }
        public string RestaurantName { get; set; }

        public long CustomerId { get; set; }
        public string CustomerName { get; set; }

        public long TotalItems { get; set; }
        public decimal TotalAmount { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string DeliveryAddress { get; set; }
    }
}
