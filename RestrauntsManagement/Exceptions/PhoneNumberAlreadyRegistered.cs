using System;

namespace DotNetRestaurantManagement.Exceptions
{
    public class PhoneNumberAlreadyRegistered : Exception
    {
        public PhoneNumberAlreadyRegistered() : base("Phone Number already Registered!") { }
    }
}