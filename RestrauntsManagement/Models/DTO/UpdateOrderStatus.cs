using DotNetRestaurantManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class UpdateOrderStatus
    {
        public long OrderId { get; set; }
        [Required]
        public OrderStatus Status { get; set; }
    }
}
