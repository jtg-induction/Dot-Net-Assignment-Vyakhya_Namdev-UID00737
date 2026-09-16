using DotNetRestaurantManagement.Models.Enums;
using System.Collections.Generic;
namespace DotNetRestaurantManagement.Models.DTO
{
    public class OrderDetailsResponse { 
        public long OrderId { get; set; } 
        public List<OrderItemResponse> Items { get; set; } 
        public long DeliveryAddress { get; set; } 
        public decimal TotalAmount { get; set; } 
        public int TotalItems { get; set; } 
        public OrderStatus OrderStatus { get; set; } 
        public long RestaurantId { get; set; } 
    }
}
