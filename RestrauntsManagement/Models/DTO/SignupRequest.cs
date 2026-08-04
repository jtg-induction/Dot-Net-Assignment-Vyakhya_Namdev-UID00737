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

        [Required]
        [MaxLength(ValidationConstants.HouseNumberMaxLength)]
        public string HouseNumber { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxTextLength)]
        public string StreetAddress { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxLocationLength)]
        public string City { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxLocationLength)]
        public string State { get; set; }

        [Required]
        [RegularExpression(RegexConstants.PinCodeRegex, ErrorMessage = ErrorMessages.InvalidPinCode)]
        public string PinCode { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxLocationLength)]
        public string Country { get; set; }

        [Required]
        [EnumDataType(typeof(AddressType))]
        public AddressType AddressType { get; set; }
    }
}
