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
    /// <summary>
    /// Validates request model data and returns a bad request response when validation fails.
    /// </summary>
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Checks the request body and model state before the controller action is executed.
        /// </summary>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var bodyParameter = actionContext.ActionDescriptor
                    .GetParameters()
                    .FirstOrDefault(x =>
                    x.GetCustomAttributes<FromBodyAttribute>().Any());

            var parameterName = bodyParameter?.ParameterName;
            if (bodyParameter != null)
            {
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
                    group => RemoveRequestPrefix(group.Key, parameterName),
                    group => GetErrorMessages(
                        RemoveRequestPrefix(group.Key, parameterName),
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

        /// <summary>
        /// Removes the request parameter name from the beginning of a model property name.
        /// </summary>
        private static string RemoveRequestPrefix(
            string key,
            string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                return key;
            }

            var prefix = parameterName + ".";
            return key.StartsWith(prefix)
                ? key.Substring(prefix.Length)
                : key;
        }

        /// <summary>
        /// Gets the validation error messages for a specific field.
        /// </summary>
        private static List<string> GetErrorMessages(string fieldName, IEnumerable<System.Web.Http.ModelBinding.ModelError> errors)
        {
            return errors
                .Select(error =>
                {
                    if (error.Exception != null)
                    {
                        return string.Format(
                                ErrorMessages.InvalidFieldValue,
                                fieldName);
                    }

                    return error.ErrorMessage;
                })
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct()
                .ToList();
        }
    }
}
