using DotNetRestaurantManagement.Constants;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class RefreshTokenRequest
    {
        [Required]
        [StringLength(ValidationConstants.RefreshTokenLength, MinimumLength = ValidationConstants.RefreshTokenLength)]
        [RegularExpression(RegexConstants.RefreshTokenRegex)]
        public string RefreshToken { get; set; }
    }
}
