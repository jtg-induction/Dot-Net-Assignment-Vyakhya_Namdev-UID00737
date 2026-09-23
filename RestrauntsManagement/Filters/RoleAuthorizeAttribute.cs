using DotNetRestaurantManagement.Models.Enums;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace DotNetRestaurantManagement.Filters
{
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _allowedRoles;
        public RoleAuthorizeAttribute(params UserRole[] roles)
        {
            _allowedRoles = roles
                .Select(role => role.ToString())
                .ToArray();
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var principal = actionContext.RequestContext.Principal;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            return _allowedRoles.Any(principal.IsInRole);
        }
    }
}
