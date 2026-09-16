using DotNetRestaurantManagement.Models.Enums;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class CancelOrderResponse
    {
        public long OrderId { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
