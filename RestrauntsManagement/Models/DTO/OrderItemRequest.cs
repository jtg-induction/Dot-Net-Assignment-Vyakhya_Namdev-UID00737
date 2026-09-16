using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class OrderItemRequest
    {
        [Required]
        public long MenuItemId { get; set; }
        [Required]
        public long RestaurantId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
