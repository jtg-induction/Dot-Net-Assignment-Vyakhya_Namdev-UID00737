using System;

namespace DotNetRestaurantManagement.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException() : base("User with this email already exists!") { }
        public DuplicateEmailException(string message) : base(message) { }
    }
}
