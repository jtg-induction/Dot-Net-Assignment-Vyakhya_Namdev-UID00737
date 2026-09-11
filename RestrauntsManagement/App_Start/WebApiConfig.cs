using DotNetRestaurantManagement.Filters;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace DotNetRestaurantManagement
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Global model validation
            config.Filters.Add(new ValidateModelStateAttribute());

            // Global exception handling
            config.Services.Replace(
                typeof(IExceptionHandler),
                new GlobalExceptionHandler());
        }
    }
}
