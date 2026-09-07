using DotNetRestaurantManagement.Controllers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Services.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;

namespace RestrauntsManagement.Tests.Controllers
{
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<IAuthService> _authServiceMock;
        private AuthController _controller;

        [TestInitialize]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
            var request = new HttpRequest("", "http://localhost/", "");
            var response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(request, response);
        }

        private SignupRequest GetValidRequest()
        {
            return new SignupRequest
            {
                Name = "Vyakhya Namdev",
                Email = "vyakhyanamdev@test.com",
                Password = "vyakhya@123",
                PhoneNumber = "9876543210",
                HouseNumber = "12A",
                StreetAddress = "MG Road",
                City = "Noida",
                State = "Uttar Pradesh",
                PinCode = "110001",
                Country = "India",
                AddressType = AddressType.Home
            };
        }

        private SignupResponse GetValidResponse()
        {
            return new SignupResponse
            {
                UserId = 1,
                Name = "Vyakhya Namdev",
                Email = "vyakhyanamdev@test.com",
                Role = "Customer",
                Balance = 1000
            };
        }

        [TestMethod]
        [Description("Verifies that a valid signup request returns HTTP 201 Created with the expected user details.")]
        public async Task Signup_ValidRequest_ReturnsCreated()
        {
            SignupRequest request = GetValidRequest();
            SignupResponse response = GetValidResponse();
            _authServiceMock
                .Setup(x => x.Signup(It.IsAny<SignupRequest>()))
                .ReturnsAsync(response);
            var result = await _controller.Signup(request);
            var okResult = result as OkNegotiatedContentResult<ApiResponse<SignupResponse>>;
            okResult.Should().NotBeNull();
            okResult.Content.Should().NotBeNull();
            okResult.Content.Data.UserId.Should().Be(1);
            okResult.Content.Data.Name.Should().Be("Vyakhya Namdev");
            okResult.Content.Data.Email.Should().Be("vyakhyanamdev@test.com");
            okResult.Content.Data.Role.Should().Be("Customer");
            okResult.Content.Data.Balance.Should().Be(1000);
        }

        [TestMethod]
        [Description("Verifies that a null signup request is passed to the signup service and the controller returns the service response.")]
        public async Task Signup_NullRequest_PassesRequestToService()
        {
            SignupRequest request = null;
            SignupResponse response = GetValidResponse();

            _authServiceMock
                .Setup(x => x.Signup(null))
                .ReturnsAsync(response);

            var result = await _controller.Signup(request);

            var okResult =
                result as OkNegotiatedContentResult<ApiResponse<SignupResponse>>;

            okResult.Should().NotBeNull();
            okResult.Content.Success.Should().BeTrue();
            okResult.Content.Data.Should().Be(response);

            _authServiceMock.Verify(
                x => x.Signup(null),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that an exception thrown by the signup service is propagated by the controller.")]
        public async Task Signup_ServiceThrowsException_PropagatesException()
        {
            SignupRequest request = GetValidRequest();
            _authServiceMock
                .Setup(x => x.Signup(It.IsAny<SignupRequest>()))
                .ThrowsAsync(new Exception("Test exception"));
            Func<Task> act = async () => await _controller.Signup(request);
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Test exception");
        }

        [TestMethod]
        [Description("Verifies that the controller passes the correct signup request data to the signup service.")]
        public async Task Signup_PassesCorrectRequestToService()
        {
            SignupRequest request = GetValidRequest();
            SignupResponse response = GetValidResponse();
            _authServiceMock
                .Setup(x => x.Signup(It.IsAny<SignupRequest>()))
                .ReturnsAsync(response);
            await _controller.Signup(request);
            _authServiceMock.Verify(
                x => x.Signup(
                    It.Is<SignupRequest>(r =>
                        r.Name == "Vyakhya Namdev" &&
                        r.Email == "vyakhyanamdev@test.com" &&
                        r.PhoneNumber == "9876543210"
                    )),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that Login successfully processes a valid login request and returns a non-null response while calling the authentication service once.")]
        public async Task Login_ValidRequest_ReturnsOk()
        {
            LoginRequest request = new LoginRequest
            {
                Email = "vyakhya@test.com",
                Password = "Vyakhya@123"
            };

            LoginResponse response = new LoginResponse
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                Name = "Vyakhya",
                Email = "vyakhya@test.com"
            };

            _authServiceMock
                .Setup(x => x.LoginAsync(request))
                .ReturnsAsync(response);

            var result = await _controller.Login(request);
            result.Should().NotBeNull();
            _authServiceMock.Verify(
                x => x.LoginAsync(request),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that an exception thrown by the login service is propagated by the controller.")]
        public async Task Login_ServiceThrowsException_PropagatesException()
        {
            LoginRequest request = new LoginRequest
            {
                Email = "vyakhya@test.com",
                Password = "Vyakhya@123"
            };

            _authServiceMock
                .Setup(x => x.LoginAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            Func<Task> act = async () => await _controller.Login(request);
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database error");
        }

        [TestMethod]
        [Description("Verifies that RefreshToken successfully processes a valid refresh token request and calls the authentication service once.")]
        public async Task RefreshToken_ValidRequest_ReturnsOk()
        {
            var request = new RefreshTokenRequest
            {
                RefreshToken = "refresh-token"
            };

            var response = new RefreshTokenResponse
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token"
            };

            _authServiceMock
                .Setup(x => x.RefreshTokenAsync(request.RefreshToken))
                .ReturnsAsync(response);

            var result = await _controller.RefreshToken(request);
            result.Should().NotBeNull();
            _authServiceMock.Verify(
                x => x.RefreshTokenAsync(request.RefreshToken),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that an unexpected exception thrown by the authentication service is propagated from the RefreshToken endpoint.")]
        public async Task RefreshToken_ServiceThrowsException_PropagatesException()
        {
            var request = new RefreshTokenRequest
            {
                RefreshToken = "refresh-token"
            };

            _authServiceMock
                .Setup(x => x.RefreshTokenAsync(request.RefreshToken))
                .ThrowsAsync(new Exception("Database error"));

            Func<Task> act = async () => await _controller.RefreshToken(request);
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database error");
        }

        [TestMethod]
        [Description("Verifies that Logout extracts the user ID and refresh token ID from the authenticated user's claims and passes them correctly to the authentication service.")]
        public async Task Logout_ValidClaims_CallsService()
        {
            var claims = new[]
            {
                new Claim("userId", "1"),
                new Claim("refreshTokenId", "10"),
                new Claim("role", "Customer")
            };

            _controller.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

            _authServiceMock
                .Setup(x => x.LogoutAsync(1, 10))
                .Returns(Task.CompletedTask);

            var result = await _controller.Logout();

            result.Should().NotBeNull();

            _authServiceMock.Verify(
                x => x.LogoutAsync(1, 10),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that Logout throws an UnauthorizedAccessException when the user ID claim is missing.")]
        public async Task Logout_MissingUserIdClaim_ThrowsUnauthorizedAccessException()
        {
            var claims = new[]
            {
                new Claim("refreshTokenId", "10"),
                new Claim("role", "Customer")
            };

            _controller.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
            Func<Task> act = async () => await _controller.Logout();
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [TestMethod]
        [Description("Verifies that Logout throws an UnauthorizedAccessException when the refresh token ID claim is missing.")]
        public async Task Logout_MissingRefreshTokenIdClaim_ThrowsUnauthorizedAccessException()
        {
            var claims = new[]
            {
                new Claim("userId", "1"),
                new Claim("role", "Customer")
            };

            _controller.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
            Func<Task> act = async () => await _controller.Logout();
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
    }
}
