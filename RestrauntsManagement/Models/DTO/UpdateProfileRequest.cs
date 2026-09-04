using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class UpdateProfileRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [Phone]
        [StringLength(15)]
        public string PhoneNumber { get; set; }
    }
}
