using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Models.Enums;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace DotNetRestaurantManagement.Filters
{
    /// <summary>
    /// Authorizes API requests using a JWT bearer token and optionally checks the user's role.
    /// </summary>
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _allowedRoles;
        /// <summary>
        /// Creates the authorization attribute with the roles allowed to access the endpoint.
        /// </summary>
        public JwtAuthorizeAttribute(params UserRole[] roles)
        {
            _allowedRoles = roles
                .Select(role => role.ToString())
                .ToArray();
        }

        /// <summary>
        /// Validates the JWT token and checks whether the user has the required role.
        /// </summary>
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            try
            {
                var authHeader = HttpContext.Current.Request.Headers[StringConstants.Authorization];

                if (string.IsNullOrWhiteSpace(authHeader) ||
                    !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();

                if (string.IsNullOrWhiteSpace(token))
                {
                    return false;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(
                    ConfigurationManager.AppSettings[StringConstants.SecretJwt]);

                SecurityToken validatedToken;

                var userInfo = tokenHandler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuer = true,
                        ValidIssuer = ConfigurationManager.AppSettings["JwtIssuer"],
                        ValidateAudience = true,
                        ValidAudience = ConfigurationManager.AppSettings["JwtAudience"]
                    },
                    out validatedToken);

                HttpContext.Current.User = userInfo;
                Thread.CurrentPrincipal = userInfo;

                if (_allowedRoles.Length == 0)
                {
                    return true;
                }

                return _allowedRoles.Any(userInfo.IsInRole);
            }
            catch
            {
                return false;
            }
        }
    }
}
