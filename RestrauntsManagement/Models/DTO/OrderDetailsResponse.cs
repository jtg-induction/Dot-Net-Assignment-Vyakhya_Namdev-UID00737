using DotNetRestaurantManagement.Models.Enums;
using System.Collections.Generic;
namespace DotNetRestaurantManagement.Models.DTO
{
    public class OrderDetailsResponse { 
        public long OrderId { get; set; } 
        public List<OrderItemResponse> Items { get; set; } 
        public AddressResponse DeliveryAddress { get; set; } 
        public decimal TotalAmount { get; set; } 
        public long TotalItems { get; set; } 
        public OrderStatus OrderStatus { get; set; } 
        public RestaurantResponse Restaurant { get; set; }
    }
}
