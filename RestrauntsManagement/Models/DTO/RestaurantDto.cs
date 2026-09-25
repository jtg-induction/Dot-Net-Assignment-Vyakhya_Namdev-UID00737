using DotNetRestaurantManagement.Models.Enums;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class RestaurantDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public RestaurantAddressDto Address { get; set; }
        public CuisineType Cuisine { get; set; }
    }
}
