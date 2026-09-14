using DotNetRestaurantManagement.Constants;
using System;

namespace DotNetRestaurantManagement.Helpers
{
    public static class PasswordHashingHelper
    {
        private const int CostFactor = 12;

        public static string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(ErrorMessages.PasswordEmptyError, nameof(password));
            }

            return BCrypt.Net.BCrypt.HashPassword(password, CostFactor);
        }

        public static bool Verify(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
