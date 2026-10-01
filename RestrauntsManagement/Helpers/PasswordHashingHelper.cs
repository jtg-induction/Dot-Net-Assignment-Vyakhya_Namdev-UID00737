using DotNetRestaurantManagement.Constants;
using System;

namespace DotNetRestaurantManagement.Helpers
{

    /// <summary>
    /// Provides methods for hashing and verifying passwords
    /// </summary>
    public static class PasswordHashingHelper
    {
        private const int CostFactor = 12;

        /// <summary>
        /// Hashes the given password using BCrypt
        /// </summary>
        public static string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(ErrorMessages.PasswordEmptyError, nameof(password));
            }

            return BCrypt.Net.BCrypt.HashPassword(password, CostFactor);
        }

        /// <summary>
        /// Verifies a password against its stored BCrypt hash
        /// </summary>
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
