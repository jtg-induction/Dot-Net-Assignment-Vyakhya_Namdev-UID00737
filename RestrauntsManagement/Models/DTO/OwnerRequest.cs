using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DotNetRestaurantManagement.Constants;

namespace DotNetRestaurantManagement.Models.DTO
{
    public class OwnerRequest
    {
        public long? UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
    }
}
