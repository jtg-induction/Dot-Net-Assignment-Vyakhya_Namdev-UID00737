using DotNetRestaurantManagement.Constants;
using System.ComponentModel.DataAnnotations;
namespace DotNetRestaurantManagement.Models.DTO
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(ValidationConstants.MaximumPasswordLength, MinimumLength = ValidationConstants.MinimumPasswordLength)]
        public string NewPassword { get; set; }
    }
}
