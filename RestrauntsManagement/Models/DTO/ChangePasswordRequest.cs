using DotNetRestaurantManagement.Constants;
using System.ComponentModel.DataAnnotations;
namespace DotNetRestaurantManagement.Models.DTO
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(ValidationConstants.MinimumPasswordLength)]
        [MaxLength(ValidationConstants.MaximumPasswordLength)]
        [RegularExpression(RegexConstants.PasswordRegex, ErrorMessage = ErrorMessages.InvalidPassword)]
        public string NewPassword { get; set; }
    }
}
