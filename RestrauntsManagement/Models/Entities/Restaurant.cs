using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetRestaurantManagement.Models.Entities
{
    public class Restaurant : BaseEntity
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
        [Index(IndexConstants.RestaurantEmailIndex, IsUnique = true)]
        public string Email { get; set; }
        [Required]
        public long OwnerId { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxNameLength)]
        public string Name { get; set; }
        [Required]
        public long AddressId { get; set; }
        public bool IsActive { get; set; } = true;

        public CuisineType Cuisine { get; set; }
        public virtual User Owner { get; set; }
        public virtual Address Address { get; set; }

        public virtual ICollection<MenuItem> MenuItems { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
