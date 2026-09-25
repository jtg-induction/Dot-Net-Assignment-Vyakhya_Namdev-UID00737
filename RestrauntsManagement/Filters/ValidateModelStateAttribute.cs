using DotNetRestaurantManagement.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace DotNetRestaurantManagement.Filters
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var bodyParameter = actionContext.ActionDescriptor
                    .GetParameters()
                    .FirstOrDefault(x =>
                    x.GetCustomAttributes<FromBodyAttribute>().Any());

            if (bodyParameter != null)
            {
                var parameterName = bodyParameter.Prefix ?? bodyParameter.ParameterName;
                if (!actionContext.ActionArguments.TryGetValue(parameterName, out var value) || value == null)
                {
                    actionContext.Response = actionContext.Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new
                        {
                            Errors = new Dictionary<string, string>
                            {
                                {
                                    StringConstants.Request, ErrorMessages.RequestBodyCannotBeEmpty
                                }
                            }
                        }
                    );

                    return;
                }
            }

            if (actionContext.ModelState.IsValid) return;
            var errors = actionContext.ModelState
                .Where(x => x.Value.Errors.Any())
                .GroupBy(x => x.Key)
                .ToDictionary(
                    group => group.Key,
                    group => GetErrorMessages(
                        group.SelectMany(x => x.Value.Errors)
                    )
                );

            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.BadRequest,
                new
                {
                    Errors = errors
                }
            );
        }

        private static List<string> GetErrorMessages(IEnumerable<System.Web.Http.ModelBinding.ModelError> errors)
        {
            return errors
                .Select(error =>
                    !string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? error.ErrorMessage
                    : error.Exception?.Message)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct()
                .ToList();
        }
    }
}
