using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class AddressRequest
    {
        [Required]
        [StringLength(50)]
        public string HouseNumber { get; set; }

        [Required]
        [StringLength(255)]
        public string StreetAddress { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(100)]
        public string State { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string PinCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }

        public int AddressType { get; set; }
    }
}
