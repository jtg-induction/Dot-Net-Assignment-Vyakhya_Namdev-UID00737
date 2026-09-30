using System;
using System.Net;

namespace DotNetRestaurantManagement.Exceptions
{
    /// <summary>
    /// Represents an application-specific exception with an HTTP status code
    /// </summary>
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        /// <summary>
        /// Creates an API exception with the specified status code and error message
        /// </summary>
        public ApiException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
