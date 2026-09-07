using DotNetRestaurantManagement.Models.Enums;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class MenuItemDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int PreparationTime { get; set; }

        public MenuCategory Category { get; set; }

        public int QuantityAvailable { get; set; }
    }
}
