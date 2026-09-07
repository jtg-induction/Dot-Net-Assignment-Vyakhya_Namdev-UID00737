using DotNetRestaurantManagement.Data;
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
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly RestaurantDbContext _context;
        public JwtAuthorizeAttribute()
        {
            _context = new RestaurantDbContext();
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            try
            {
                var cookie = HttpContext.Current.Request.Cookies["AccessToken"];
                if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value)) return false;
                string token = cookie.Value;
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["JwtSecret"]));
                SecurityToken validatedToken;
                var userInfo = tokenHandler.ValidateToken(
                    cookie.Value,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    },
                    out validatedToken);

                var userIdClaim = 
                    userInfo.FindFirst(JwtRegisteredClaimNames.Sub)
                    ?? userInfo.FindFirst(ClaimTypes.NameIdentifier)
                    ?? userInfo.FindFirst("sub");
                if (userIdClaim == null) return false;
                long userId = Convert.ToInt64(userIdClaim.Value);
                var user = _context.Users.FirstOrDefault(x => x.Id == userId);
                if (user == null || !user.IsActive) return false;
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
