using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetRestaurantManagement.Models.Entities
{
    public class User : BaseEntity
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
        [MaxLength(ValidationConstants.MaxTextLength)]
        [Index(IndexConstants.UserEmailIndex, IsUnique = true)]
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
        [RegularExpression(RegexConstants.PhoneNumberRegex, ErrorMessage = ErrorMessages.InvalidPhoneNumber)]
        [Index(IndexConstants.UserPhoneNumberIndex, IsUnique = true)]
        public string PhoneNumber { get; set; }

        public decimal Balance { get; set; } = ValidationConstants.DefaultUserBalance;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserAddress> UserAddresses { get; set; }

        public virtual ICollection<Restaurant> Restaurants { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
