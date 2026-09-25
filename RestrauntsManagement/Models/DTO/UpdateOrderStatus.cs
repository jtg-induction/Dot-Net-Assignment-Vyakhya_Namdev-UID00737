using DotNetRestaurantManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class UpdateOrderStatus
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}
