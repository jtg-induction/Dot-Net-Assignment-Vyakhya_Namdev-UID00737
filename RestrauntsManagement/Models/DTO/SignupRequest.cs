using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class SignupRequest
    {
        [Required]
        [MaxLength(ValidationConstants.MaxTextLength)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(ValidationConstants.MaxTextLength)]
        public string Email {  get; set; }

        [Required]
        [MinLength(ValidationConstants.MinimumPasswordLength)]
        [MaxLength(ValidationConstants.MaximumPasswordLength)]
        [RegularExpression(RegexConstants.PasswordRegex, ErrorMessage = ErrorMessages.InvalidPassword)]
        public string Password { get; set; }

        [Required]
        [RegularExpression(RegexConstants.PhoneNumberRegex, ErrorMessage = ErrorMessages.InvalidPhoneNumber)]
        public string PhoneNumber { get; set; }
    }
}
