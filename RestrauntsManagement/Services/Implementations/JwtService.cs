using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DotNetRestaurantManagement.Services.Implementations
{
    public class JwtService : IJwtService
    {
        private readonly string _secret;
        private readonly int _accessTokenExpiryTime;

        public JwtService()
        {
            _secret = ConfigurationManager.AppSettings["JwtSecret"];
            _accessTokenExpiryTime = Convert.ToInt32(ConfigurationManager.AppSettings["AccessTokenExpiryMinutes"]);
        }

        public string GenerateAccessToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryTime),
            signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            byte[] randomBytes = new byte[64];
            using (var random = RandomNumberGenerator.Create()){
                random.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
}
