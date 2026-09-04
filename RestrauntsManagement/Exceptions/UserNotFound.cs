using System;

namespace DotNetRestaurantManagement.Exceptions
{
    public class UserNotFound : Exception
    {
        public UserNotFound() : base("User Not Found!") {}
    }
}