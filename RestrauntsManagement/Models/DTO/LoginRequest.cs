using DotNetRestaurantManagement.Constants;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(ValidationConstants.MaxTextLength)]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
