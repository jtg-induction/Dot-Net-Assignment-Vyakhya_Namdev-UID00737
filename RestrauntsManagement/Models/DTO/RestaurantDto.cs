using DotNetRestaurantManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class RestaurantDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public RestaurantAddressDto Address { get; set; }
        public string Cuisine { get; set; }
    }
}
