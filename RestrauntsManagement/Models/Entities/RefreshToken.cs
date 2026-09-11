using System;

namespace DotNetRestaurantManagement.Models.Entities
{
    public class RefreshToken
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Token {  get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual User User { get; set; }

    }
}
