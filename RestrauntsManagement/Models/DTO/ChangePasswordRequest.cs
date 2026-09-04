using System.ComponentModel.DataAnnotations;
namespace DotNetRestaurantManagement.Models.DTO
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; }
    }
}
