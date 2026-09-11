using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Claims;

namespace RestrauntsManagement.Tests.Helpers
{
    [TestClass]
    public class ClaimsHelperTests
    {
        [TestMethod]
        [Description("Verifies that GetUserId returns the correct user ID when a valid user ID claim is present.")]
        public void GetUserId_ValidClaim_ReturnsUserId()
        {
            var claims = new[]
            {
                new Claim(StringConstants.UserId, "1")
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
            long result = ClaimsHelper.GetUserId(user);
            result.Should().Be(1);
        }

        [TestMethod]
        [Description("Verifies that GetUserId throws an UnauthorizedAccessException when the user ID claim is missing.")]
        public void GetUserId_MissingClaim_ThrowsUnauthorizedAccessException()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            Action act = () => ClaimsHelper.GetUserId(user);

            act.Should()
                .Throw<UnauthorizedAccessException>()
                .WithMessage(ErrorMessages.UserIdMissingClaim);
        }

        [TestMethod]
        [Description("Verifies that GetUserId throws an UnauthorizedAccessException when the user ID claim contains an invalid value.")]
        public void GetUserId_InvalidClaim_ThrowsUnauthorizedAccessException()
        {
            var claims = new[]
            {
                new Claim(StringConstants.UserId, "invalid")
            };

            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
            Action act = () => ClaimsHelper.GetUserId(user);

            act.Should()
                .Throw<UnauthorizedAccessException>()
                .WithMessage(ErrorMessages.UserIdMissingClaim);
        }

        [TestMethod]
        [Description("Verifies that GetUserId throws an UnauthorizedAccessException when the principal is null.")]
        public void GetUserId_NullPrincipal_ThrowsUnauthorizedAccessException()
        {
            Action act = () => ClaimsHelper.GetUserId(null);
            act.Should()
                .Throw<UnauthorizedAccessException>()
                .WithMessage(ErrorMessages.UserIdMissingClaim);
        }

        [TestMethod]
        [Description("Verifies that GetRefreshTokenId returns the correct refresh token ID when a valid refresh token ID claim is present.")]
        public void GetRefreshTokenId_ValidClaim_ReturnsRefreshTokenId()
        {
            var claims = new[]
            {
                new Claim(StringConstants.RefreshTokenId, "10")
            };

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(claims));

            long result = ClaimsHelper.GetRefreshTokenId(user);

            result.Should().Be(10);
        }

        [TestMethod]
        [Description("Verifies that GetRefreshTokenId throws an UnauthorizedAccessException when the refresh token ID claim is missing.")]
        public void GetRefreshTokenId_MissingClaim_ThrowsUnauthorizedAccessException()
        {
            var user = new ClaimsPrincipal(
                new ClaimsIdentity());

            Action act = () => ClaimsHelper.GetRefreshTokenId(user);

            act.Should()
                .Throw<UnauthorizedAccessException>()
                .WithMessage(ErrorMessages.RefreshTokenIdMissingClaim);
        }

        [TestMethod]
        [Description("Verifies that GetRefreshTokenId throws an UnauthorizedAccessException when the principal is null.")]
        public void GetRefreshTokenId_NullPrincipal_ThrowsUnauthorizedAccessException()
        {
            Action act = () => ClaimsHelper.GetRefreshTokenId(null);

            act.Should()
                .Throw<UnauthorizedAccessException>()
                .WithMessage(ErrorMessages.RefreshTokenIdMissingClaim);
        }

        [TestMethod]
        [Description("Verifies that GetRefreshTokenId returns the correct refresh token ID when multiple claims are present.")]
        public void GetRefreshTokenId_MultipleClaims_ReturnsCorrectRefreshTokenId()
        {
            var claims = new[]
            {
                new Claim(StringConstants.UserId, "25"),
                new Claim(StringConstants.RefreshTokenId, "100"),
                new Claim(StringConstants.userRole, "Customer")
            };

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(claims));

            long result = ClaimsHelper.GetRefreshTokenId(user);

            result.Should().Be(100);
        }
    }
}
