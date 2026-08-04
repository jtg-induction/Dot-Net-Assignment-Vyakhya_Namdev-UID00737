using DotNetRestaurantManagement.Models.DTO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace DotNetRestaurantManagement
{
    public class ErrorResponseResult : IHttpActionResult
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _message;
        private readonly HttpRequestMessage _request;

        public ErrorResponseResult(
            HttpStatusCode statusCode,
            string message,
            HttpRequestMessage request)
        {
            _statusCode = statusCode;
            _message = message;
            _request = request;
        }

        public Task<HttpResponseMessage> ExecuteAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                _request.CreateResponse(
                    _statusCode,
                    new ErrorResponse
                    {
                        Message = _message
                    }));
        }
    }
}