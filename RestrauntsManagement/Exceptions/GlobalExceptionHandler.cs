using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace DotNetRestaurantManagement
{
    /// <summary>
    /// Handles unhandled exceptions and returns a consistent API error response
    /// </summary>
    public class GlobalExceptionHandler : ExceptionHandler
    {
        /// <summary>
        /// Handles the exception and creates an API response with the appropriate HTTP status code and message
        /// </summary>
        public override Task HandleAsync(
            ExceptionHandlerContext context,
            CancellationToken cancellationToken)
        {
            Exception exception = context.Exception;
            HttpStatusCode statusCode;
            string message;

            if (exception is ApiException apiException)
            {
                statusCode = apiException.StatusCode;
                message = apiException.Message;
            }
            else
            {
                statusCode = HttpStatusCode.InternalServerError;
                message = exception.Message;
            }

            var response = new ApiResponse<object>(
                false, 
                null,
                message
            );

            context.Result = new ResponseMessageResult(
                context.Request.CreateResponse(
                    statusCode,
                    response
                )
            );

            return Task.CompletedTask;
        }
    }
}
