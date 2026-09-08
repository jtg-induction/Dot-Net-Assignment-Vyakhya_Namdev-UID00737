using DotNetRestaurantManagement.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetRestaurantManagement.Models.Entities
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public long MenuItemId { get; set; }

        public long OrderId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(typeof(decimal), ValidationConstants.MinimumPrice, ValidationConstants.MaximumPrice)]
        public decimal Price { get; set; }
        public virtual MenuItem MenuItem { get; set; }
        public virtual Order Order { get; set; }
    }
}
