using DotNetRestaurantManagement.Constants;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace DotNetRestaurantManagement.Filters
{
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {
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

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
