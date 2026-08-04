using System;

namespace DotNetRestaurantManagement.Exceptions
{
    public class DuplicatePhoneNumberException : Exception
    {
        public DuplicatePhoneNumberException() : base("User with this phone number already exists!") { }
        public DuplicatePhoneNumberException(string message) : base(message) { }
    }
}
