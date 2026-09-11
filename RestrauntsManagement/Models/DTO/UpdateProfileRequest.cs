using DotNetRestaurantManagement.Constants;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class UpdateProfileRequest
    {
        [StringLength(ValidationConstants.MaxNameLength)]
        public string Name { get; set; }

        [Phone]
        [StringLength(ValidationConstants.PhoneNumberLength)]
        public string PhoneNumber { get; set; }
    }
}
