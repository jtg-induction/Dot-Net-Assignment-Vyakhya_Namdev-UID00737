using System;

namespace DotNetRestaurantManagement.Exceptions
{
    public class InvalidRefreshTokenException : Exception
    {
        public InvalidRefreshTokenException() : base("Invalid or expired refresh token!") { }
    }
}
