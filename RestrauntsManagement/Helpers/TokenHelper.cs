using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Models.Entities;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Configuration;

namespace DotNetRestaurantManagement.Helpers
{
    public class TokenHelper
    {
        private static readonly string _secret = ConfigurationManager.AppSettings[StringConstants.SecretJwt];
        private static readonly int _accessTokenExpiryTime = Convert.ToInt32(ConfigurationManager.AppSettings[StringConstants.AccessTokenExpiry]);
        public static string Hash(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException(
                    ErrorMessages.RefreshTokenEmptyValidation,
                    nameof(refreshToken));
            }

            using (var sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(refreshToken);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                return BitConverter
                    .ToString(hashBytes)
                    .Replace("-", "")
                    .ToLowerInvariant();
            }
        }

        public static string GenerateAccessToken(User user, long refreshTokenId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(StringConstants.RefreshTokenId, refreshTokenId.ToString()),
                new Claim(StringConstants.UserId, user.Id.ToString()),
                new Claim(StringConstants.userRole, user.Role.ToString())
            };

            string issuer = ConfigurationManager.AppSettings["JwtIssuer"];
            string audience = ConfigurationManager.AppSettings["JwtAudience"];
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryTime),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GenerateRefreshToken()
        {
            byte[] randomBytes = new byte[64];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
}
