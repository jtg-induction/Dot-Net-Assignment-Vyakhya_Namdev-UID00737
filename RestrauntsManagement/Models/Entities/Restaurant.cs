using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetRestaurantManagement.Models.Entities
{
    public class Restaurant
    {
        public Restaurant()
        {
            MenuItems = new HashSet<MenuItem>();
            Orders = new HashSet<Order>();
        }

        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxTextLength)]
        [Index("Restaurants_Email", IsUnique = true)]
        public string Email { get; set; }

        public long OwnerId { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxNameLength)]
        public string Name { get; set; }

        public long AddressId { get; set; }
        public bool IsActive { get; set; } = true;

        public CuisineType Cuisine { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public virtual User Owner { get; set; }
        public virtual Address Address { get; set; }

        public virtual ICollection<MenuItem> MenuItems { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
