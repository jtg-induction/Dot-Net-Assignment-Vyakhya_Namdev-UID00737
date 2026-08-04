using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Linq;

namespace DotNetRestaurantManagement.Filters
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ModelState.IsValid) return;
            var errors = actionContext.ModelState
                .Where(x => x.Value.Errors.Any())
                .SelectMany(x => x.Value.Errors.Select(error =>
                new
                {
                    Field = x.Key,
                    Message = !string.IsNullOrEmpty(error.ErrorMessage) ? error.ErrorMessage : error.Exception?.Message
                }))
                .ToList();

            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.BadRequest,
                new
                {
                    Errors = errors
                }
            );
        }
    }
}
