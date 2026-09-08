using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.Entities
{
    public class MenuItem : BaseEntity
    {
        public MenuItem()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        [Key]
        public long Id { get; set; }
        [Required]
        public long RestaurantId { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxNameLength)]
        public string Name { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int PreparationTime { get; set; }
        public MenuCategory Category { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int QuantityAvailable { get; set; }
        public virtual Restaurant Restaurant { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
