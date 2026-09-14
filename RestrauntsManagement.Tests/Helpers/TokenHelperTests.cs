using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace DotNetRestaurantManagement.Tests.Helpers
{
    [TestClass]
    public class TokenHelperTests
    {
        [TestMethod]
        [Description("Verifies that hashing the same refresh token produces the same SHA-256 hash.")]
        public void Hash_SameRefreshToken_ReturnsSameHash()
        {
            string refreshToken = "sample-refresh-token";
            string firstHash = TokenHelper.Hash(refreshToken);
            string secondHash = TokenHelper.Hash(refreshToken);
            firstHash.Should().NotBeNullOrWhiteSpace();
            firstHash.Should().Be(secondHash);
        }

        [TestMethod]
        [Description("Verifies that different refresh tokens produce different SHA-256 hashes.")]
        public void Hash_DifferentRefreshTokens_ReturnsDifferentHashes()
        {
            string firstToken = "refresh-token-1";
            string secondToken = "refresh-token-2";
            string firstHash = TokenHelper.Hash(firstToken);
            string secondHash = TokenHelper.Hash(secondToken);
            firstHash.Should().NotBe(secondHash);
        }

        [TestMethod]
        [Description("Verifies that the generated SHA-256 hash is a 64-character lowercase hexadecimal string.")]
        public void Hash_ValidRefreshToken_ReturnsValidSha256Hash()
        {
            string refreshToken = "sample-refresh-token";
            string hash = TokenHelper.Hash(refreshToken);
            hash.Should().HaveLength(64);
            hash.Should().MatchRegex("^[a-f0-9]{64}$");
        }

        [TestMethod]
        [Description("Verifies that an access token is generated as a readable JWT.")]
        public void GenerateAccessToken_ValidUser_ReturnsValidJwt()
        {
            var user = new User
            {
                Id = 101,
                Role = UserRole.Customer
            };
            string accessToken = TokenHelper.GenerateAccessToken(user, 5001);
            accessToken.Should().NotBeNullOrWhiteSpace();
            var handler = new JwtSecurityTokenHandler();
            handler.CanReadToken(accessToken).Should().BeTrue();
        }

        [TestMethod]
        [Description("Verifies that the generated access token contains the user ID, refresh token ID, and role claims.")]
        public void GenerateAccessToken_ValidUser_ContainsRequiredClaims()
        {
            var user = new User
            {
                Id = 101,
                Role = UserRole.Customer
            };
            string accessToken = TokenHelper.GenerateAccessToken(user, 5001);
            var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            token.Claims.Should().ContainSingle(x => x.Type == StringConstants.UserId && x.Value == "101");
            token.Claims.Should().ContainSingle(x => x.Type == StringConstants.RefreshTokenId && x.Value == "5001");
            token.Claims.Should().ContainSingle(x => x.Type == StringConstants.userRole && x.Value == UserRole.Customer.ToString());
        }

        [TestMethod]
        [Description("Verifies that the generated access token contains an expiration time in the future.")]
        public void GenerateAccessToken_ValidUser_SetsFutureExpiration()
        {
            var user = new User
            {
                Id = 101,
                Role = UserRole.Customer
            };
            DateTime beforeGeneration = DateTime.UtcNow;
            string accessToken = TokenHelper.GenerateAccessToken(user, 5001);
            DateTime afterGeneration = DateTime.UtcNow;
            var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            token.ValidTo.Should().BeAfter(beforeGeneration);
            token.ValidTo.Should().BeBefore(afterGeneration.AddMinutes(20));
        }
    }
}
