using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class PlaceOrderRequest
    {
        [Required]
        public List<OrderItemRequest> Items { get; set; }

        [Required]
        public long DeliveryAddressId { get; set; }
        [Required]
        public long RestaurantId { get; set; }
    }
}
