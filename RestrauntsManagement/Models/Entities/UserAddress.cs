namespace DotNetRestaurantManagement.Models.Entities
{
    public class UserAddress
    {
        public long UserId { get; set; }
        public long AddressId { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual User User { get; set; }
        public virtual Address Address { get; set; }
    }
}
