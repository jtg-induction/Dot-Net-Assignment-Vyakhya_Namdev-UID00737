using DotNetRestaurantManagement.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetRestaurantManagement.Models.Entities
{
    [Table("Addresses")]
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
        [MaxLength(50)]
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
        [StringLength(6, MinimumLength = 6)]
        public string PinCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string Country { get; set; }
        public AddressType AddressType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<UserAddress> UserAddresses { get; set; }
        public virtual ICollection<Restaurant> Restaurants { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
