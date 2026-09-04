using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Services.Implementations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RestrauntsManagement.Tests.Services
{
    [TestClass]
    public class JwtServiceTests
    {
        private JwtService _jwtService;
        private User _mockUser;
        [TestInitialize]
        public void TestInitialize()
        {
            _jwtService = new JwtService();
            _mockUser = new User
            {
                Id = 1,
                Name = "Vyakhya Namdev",
                Email = "vyakhya.namdev@test.com",
                Role = UserRole.Customer,
                TokenVersion = 1
            };
        }

        [TestMethod]
        public void GenerateAccessToken_ShouldReturnValidToken()
        {
            var token = _jwtService.GenerateAccessToken(_mockUser);
            token.Should().NotBeNullOrEmpty();
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            jwtToken.Should().NotBeNull();
        }

        [TestMethod]
        public void GenerateAccessToken_ShouldContainCorrectUserClaims()
        {
            var token = _jwtService.GenerateAccessToken(_mockUser);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            jwtToken.Claims.Should().Contain(x =>
                x.Type == ClaimTypes.Name &&
                x.Value == "Vyakhya Namdev");

            jwtToken.Claims.Should().Contain(x =>
                x.Type == JwtRegisteredClaimNames.Sub &&
                x.Value == "1");

            jwtToken.Claims.Should().Contain(x =>
                x.Type == JwtRegisteredClaimNames.Email &&
                x.Value == "vyakhya.namdev@test.com");

            jwtToken.Claims.Should().Contain(x =>
                x.Type == ClaimTypes.Role &&
                x.Value == UserRole.Customer.ToString());

            jwtToken.Claims.Should().Contain(x =>
                x.Type == "TokenVersion" &&
                x.Value == "1");
        }

        [TestMethod]
        public void GenerateAccessToken_ShouldHaveExpiryTime()
        {
            var token = _jwtService.GenerateAccessToken(_mockUser);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            jwtToken.ValidTo.Should().BeAfter(System.DateTime.UtcNow);
        }

        [TestMethod]
        public void GenerateRefreshToken_ShouldReturnNonEmptyToken()
        {
            var refreshToken = _jwtService.GenerateRefreshToken();
            refreshToken.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void GenerateRefreshToken_ShouldGenerateDifferentToken()
        {
            var firstToken = _jwtService.GenerateRefreshToken();
            var secondToken = _jwtService.GenerateRefreshToken();

            firstToken.Should().NotBe(secondToken);
        }
    }
}
