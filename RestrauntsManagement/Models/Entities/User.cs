using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetRestaurantManagement.Models.Entities
{
    [Table("Users")]
    public class User
    {
        public User()
        {
            UserAddresses = new HashSet<UserAddress>();
            Restaurants = new HashSet<Restaurant>();
            Orders = new HashSet<Order>();
        }

        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Index("Users_Email", IsUnique = true)]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        [MaxLength(10)]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Please Enter Valid Number!")]
        public string PhoneNumber { get; set; }

        public decimal Balance { get; set; } = 1000;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<UserAddress> UserAddresses { get; set; }

        public virtual ICollection<Restaurant> Restaurants { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
