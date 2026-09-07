using DotNetRestaurantManagement.Constants;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class AddressRequest
    {
        [Required]
        [StringLength(ValidationConstants.HouseNumberMaxLength)]
        public string HouseNumber { get; set; }

        [Required]
        [StringLength(ValidationConstants.MaxTextLength)]
        public string StreetAddress { get; set; }

        [Required]
        [StringLength(ValidationConstants.MaxLocationLength)]
        public string City { get; set; }

        [Required]
        [StringLength(ValidationConstants.MaxLocationLength)]
        public string State { get; set; }

        [Required]
        [StringLength(ValidationConstants.PinCodeLength, MinimumLength = ValidationConstants.PinCodeLength)]
        public string PinCode { get; set; }

        [Required]
        [StringLength(ValidationConstants.MaxLocationLength)]
        public string Country { get; set; }

        public int AddressType { get; set; }
    }
}
