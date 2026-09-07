using System;

namespace DotNetRestaurantManagement.Exceptions
{
    public class ValidEmailException : Exception
    {
        public ValidEmailException() : base("Please enter valid Email!") { }
    }
}
