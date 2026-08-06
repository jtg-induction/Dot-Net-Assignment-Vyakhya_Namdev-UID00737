using System;

namespace DotNetRestaurantManagement.Exceptions
{
    public class ValidPasswordException : Exception
    {
        public ValidPasswordException() : base("Please enter Valid Password!") { }
    }
}