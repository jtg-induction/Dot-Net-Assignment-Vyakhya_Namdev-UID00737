using DotNetRestaurantManagement.Services.Interfaces;
using System;

namespace DotNetRestaurantManagement.Services.Implementations
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int CostFactor = 12;
        public string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password)){
                throw new ArgumentException("Password cannot be empty!*", nameof(password));
            }
            return BCrypt.Net.BCrypt.HashPassword(password, CostFactor);
        }

        public bool Verify(string password, string passwordHash){
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash)){
                return false;
            }
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}