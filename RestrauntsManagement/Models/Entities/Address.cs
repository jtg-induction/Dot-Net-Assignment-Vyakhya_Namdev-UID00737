using DotNetRestaurantManagement.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DotNetRestaurantManagement.Constants;

namespace DotNetRestaurantManagement.Models.Entities
{
    public class Address
    {
        public Address()
        {
            UserAddresses = new HashSet<UserAddress>();
            Restaurants = new HashSet<Restaurant>();
            Orders = new HashSet<Order>();
        }

        [Key]
        public long Id { get; set; }

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
        [StringLength(ValidationConstants.PinCodeLength, MinimumLength = ValidationConstants.PinCodeLength)]
        public string PinCode { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxLocationLength)]
        public string Country { get; set; }
        public AddressType AddressType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<UserAddress> UserAddresses { get; set; }
        public virtual ICollection<Restaurant> Restaurants { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
