using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetRestaurantManagement.Models.Entities
{
    public class User
    {
        public User()
        {
            UserAddresses = new HashSet<UserAddress>();
            Restaurants = new HashSet<Restaurant>();
            Orders = new HashSet<Order>();
            RefreshTokens = new HashSet<RefreshToken>();
        }

        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxTextLength)]
        [Index("Users_Email", IsUnique = true)]
        public string Email { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxTextLength)]
        public string Name { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxTextLength)]
        public string Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        [MaxLength(ValidationConstants.PhoneNumberLength)]
        [RegularExpression(RegexConstants.PhoneNumberRegex, ErrorMessage = ErrorMessages.ValidPhoneNumber)]
        [Index("Users_PhoneNumber", IsUnique = true)]
        public string PhoneNumber { get; set; }

        public decimal Balance { get; set; } = ValidationConstants.DefaultUserBalance;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<UserAddress> UserAddresses { get; set; }

        public virtual ICollection<Restaurant> Restaurants { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
