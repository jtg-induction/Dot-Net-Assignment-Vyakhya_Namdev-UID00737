using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class OnboardRestaurantRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Cuisine { get; set; }
        [Required]
        public AddressRequest Address { get; set; }
        [Required]
        public OwnerRequest Owner { get; set; }
    }
}
