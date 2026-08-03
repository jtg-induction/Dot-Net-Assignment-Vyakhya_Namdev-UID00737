using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetRestaurantManagement.Models.Entities
{
    [Table("UserAddress")]
    public class UserAddress
    {
        public long UserId { get; set; }
        public long AddressId { get; set; }
        public virtual User User { get; set; }
        public virtual Address Address { get; set; }
    }
}
