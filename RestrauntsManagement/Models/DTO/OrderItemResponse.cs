namespace DotNetRestaurantManagement.Models.DTO
{
    public class OrderItemResponse
    {
        public long MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
