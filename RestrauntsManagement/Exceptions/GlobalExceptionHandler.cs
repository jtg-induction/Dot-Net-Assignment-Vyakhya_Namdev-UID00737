using DotNetRestaurantManagement.Exceptions;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace DotNetRestaurantManagement
{
    public class GlobalExceptionHandler : ExceptionHandler
    {
        public override Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken){
            Exception exception = context.Exception;
            HttpStatusCode statusCode;
            string message;

            if (exception is DuplicateEmailException || exception is DuplicatePhoneNumberException){
                statusCode = HttpStatusCode.Conflict;
                message = exception.Message;
            }
            else
            {
                statusCode = HttpStatusCode.InternalServerError;
                message = exception.Message;
            }

            context.Result = new ErrorResponseResult(
                statusCode,
                message,
                context.Request);

            return Task.CompletedTask;
        }
    }
}
