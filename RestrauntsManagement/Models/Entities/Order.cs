using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.Entities
{
    public class Order : BaseEntity
    {
        public Order()
        {
            OrderedItems = new HashSet<OrderItem>();
        }

        [Key]
        public long Id { get; set; }
        [Required]
        public long RestaurantId { get; set; }
        [Required]
        public long CustomerId { get; set; }
        [Required]
        public long DeliveryAddressId { get; set; }
        [Required]
        [Range(ValidationConstants.MinimumQuantity, long.MaxValue)]
        public long TotalItems { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Placed;
        public virtual Restaurant Restaurant { get; set; }
        public virtual User Customer { get; set; }
        public virtual Address DeliveryAddress { get; set; }

        public virtual ICollection<OrderItem> OrderedItems { get; set; }
    }
}
