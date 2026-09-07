using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetRestaurantManagement.Models.Entities
{
    public class RefreshToken
    {
        public long Id { get; set; }
        [Required]
        public long UserId { get; set; }
        [Required]
        public string Token {  get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public virtual User User { get; set; }

    }
}
