using DotNetRestaurantManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class SignupRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email {  get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(50)]
        public string Password { get; set; }

        [Required]
        [RegularExpression(@"^[6-9][0-9]{9}$", ErrorMessage = "Please enter valid phone number!")]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(10)]
        public string HouseNumber { get; set; }

        [Required]
        [MaxLength(255)]
        public string StreetAddress { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(100)]
        public string State { get; set; }

        [Required]
        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "Pin code must contain 6 digits!")]
        public string PinCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string Country { get; set; }

        [Required]
        [EnumDataType(typeof(AddressType))]
        public AddressType AddressType { get; set; }
    }
}