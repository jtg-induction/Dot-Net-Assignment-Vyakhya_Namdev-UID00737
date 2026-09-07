using DotNetRestaurantManagement.Controllers;
using DotNetRestaurantManagement.Exceptions;
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
using System.Web.Http;
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
                Balance = 1000,
                Message = "User registered successfully!"
            };
        }

        private void SetUser(int userId)
        {
            var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                },
                "TestAuthentication");

            _controller.User = new ClaimsPrincipal(identity);
        }

        [TestMethod]
        public async Task Signup_ValidRequest_ReturnsCreated()
        {
            // Arrange
            SignupRequest request = GetValidRequest();
            SignupResponse response = GetValidResponse();

            _authServiceMock
                .Setup(x => x.Signup(It.IsAny<SignupRequest>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Signup(request);

            // Assert
            var createdResult = result as NegotiatedContentResult<SignupResponse>;

            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(HttpStatusCode.Created);

            createdResult.Content.Should().NotBeNull();
            createdResult.Content.UserId.Should().Be(1);
            createdResult.Content.Name.Should().Be("Vyakhya Namdev");
            createdResult.Content.Email.Should().Be("vyakhyanamdev@test.com");
            createdResult.Content.Role.Should().Be("Customer");
            createdResult.Content.Balance.Should().Be(1000);
            createdResult.Content.Message.Should().Be(
                "User registered successfully!");
        }

        [TestMethod]
        public async Task Signup_ValidRequest_CallsServiceExactlyOnce()
        {
            // Arrange
            SignupRequest request = GetValidRequest();
            SignupResponse response = GetValidResponse();

            _authServiceMock
                .Setup(x => x.Signup(It.IsAny<SignupRequest>()))
                .ReturnsAsync(response);

            // Act
            await _controller.Signup(request);

            // Assert
            _authServiceMock.Verify(
                x => x.Signup(It.IsAny<SignupRequest>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Signup_NullRequest_PassesRequestToService()
        {
            // Arrange
            SignupRequest request = null;
            SignupResponse response = GetValidResponse();

            _authServiceMock
                .Setup(x => x.Signup(null))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Signup(request);

            // Assert
            var createdResult = result as NegotiatedContentResult<SignupResponse>;

            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(HttpStatusCode.Created);

            _authServiceMock.Verify(
                x => x.Signup(null),
                Times.Once);
        }

        [TestMethod]
        public async Task Signup_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            SignupRequest request = GetValidRequest();

            _authServiceMock
                .Setup(x => x.Signup(It.IsAny<SignupRequest>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            Func<Task> act = async () => await _controller.Signup(request);

            // Assert
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Test exception");
        }

        [TestMethod]
        public async Task Signup_PassesCorrectRequestToService()
        {
            // Arrange
            SignupRequest request = GetValidRequest();
            SignupResponse response = GetValidResponse();

            _authServiceMock
                .Setup(x => x.Signup(It.IsAny<SignupRequest>()))
                .ReturnsAsync(response);

            // Act
            await _controller.Signup(request);

            // Assert
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
        public async Task Login_ValidRequest_SetsAccessAndRefreshTokenCookies()
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

            await _controller.Login(request);
            var accessTokenCookie = HttpContext.Current.Response.Cookies["AccessToken"];
            var refreshTokenCookie = HttpContext.Current.Response.Cookies["RefreshToken"];
            accessTokenCookie.Should().NotBeNull();
            accessTokenCookie.Value.Should().Be("access-token");
            accessTokenCookie.HttpOnly.Should().BeTrue();
            refreshTokenCookie.Should().NotBeNull();
            refreshTokenCookie.Value.Should().Be("refresh-token");
            refreshTokenCookie.HttpOnly.Should().BeTrue();
        }

        [TestMethod]
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
        public async Task Logout_ValidRefreshToken_CallsService()
        {
            var refreshTokenCookie = new HttpCookie(
                "RefreshToken",
                "refresh-token");

            HttpContext.Current.Request.Cookies.Add(refreshTokenCookie);
            _authServiceMock
                .Setup(x => x.LogoutAsync("refresh-token"))
                .Returns(Task.CompletedTask);

            var result = await _controller.Logout();
            result.Should().NotBeNull();
            _authServiceMock.Verify(
                x => x.LogoutAsync("refresh-token"),
                Times.Once);
        }

        [TestMethod]
        public async Task Logout_ValidRefreshToken_DeletesCookies()
        {
            var refreshTokenCookie = new HttpCookie(
                "RefreshToken",
                "refresh-token");

            HttpContext.Current.Request.Cookies.Add(refreshTokenCookie);
            _authServiceMock
                .Setup(x => x.LogoutAsync("refresh-token"))
                .Returns(Task.CompletedTask);

            await _controller.Logout();
            var accessTokenCookie = HttpContext.Current.Response.Cookies["AccessToken"];
            var deletedRefreshTokenCookie = HttpContext.Current.Response.Cookies["RefreshToken"];
            accessTokenCookie.Should().NotBeNull();
            deletedRefreshTokenCookie.Should().NotBeNull();
            accessTokenCookie.Expires.Should().BeBefore(DateTime.Now);
            deletedRefreshTokenCookie.Expires.Should().BeBefore(DateTime.Now);
        }

        [TestMethod]
        public async Task Logout_ServiceThrowsException_PropagatesException()
        {
            var refreshTokenCookie = new HttpCookie(
                "RefreshToken",
                "refresh-token");

            HttpContext.Current.Request.Cookies.Add(refreshTokenCookie);
            _authServiceMock
                .Setup(x => x.LogoutAsync("refresh-token"))
                .ThrowsAsync(new Exception("Database error"));
            Func<Task> act = async () => await _controller.Logout();
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database error");
        }

        [TestMethod]
        public async Task DeactivateAccount_ValidUser_ReturnsOk()
        {
            int userId = 1;
            SetUser(userId);
            _authServiceMock
                .Setup(x => x.DeactivateAccountAsync(userId))
                .Returns(Task.CompletedTask);

            var result = await _controller.DeactivateAccount();
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<IHttpActionResult>();
            _authServiceMock.Verify(
                x => x.DeactivateAccountAsync(userId),
                Times.Once);
        }

        [TestMethod]
        public async Task DeactivateAccount_UserNotFound_PropagatesException()
        {
            int userId = 1;
            SetUser(userId);
            _authServiceMock
                .Setup(x => x.DeactivateAccountAsync(userId))
                .ThrowsAsync(new UserNotFound());

            Func<Task> act = () => _controller.DeactivateAccount();
            await act.Should().ThrowAsync<UserNotFound>();
            _authServiceMock.Verify(
                x => x.DeactivateAccountAsync(userId),
                Times.Once);
        }

        [TestMethod]
        public async Task DeactivateAccount_ServiceFails_PropagatesException()
        {
            int userId = 1;
            SetUser(userId);
            _authServiceMock
                .Setup(x => x.DeactivateAccountAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            Func<Task> act = () => _controller.DeactivateAccount();
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Database error");
        }

        [TestMethod]
        public async Task DeactivateAccount_MissingUserIdClaim_ThrowsUnauthorizedAccessException()
        {
            var identity = new ClaimsIdentity(authenticationType: "TestAuthentication");
            _controller.User = new ClaimsPrincipal(identity);
            Func<Task> act = () => _controller.DeactivateAccount();
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _authServiceMock.Verify(
                x => x.DeactivateAccountAsync(It.IsAny<int>()),
                Times.Never);
        }

        [TestMethod]
        public async Task DeactivateAccount_InvalidUserIdClaim_ThrowsUnauthorizedAccessException()
        {
            var identity = new ClaimsIdentity( new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "invalid-id")
                },
                "TestAuthentication");

            _controller.User = new ClaimsPrincipal(identity);
            Func<Task> act = () => _controller.DeactivateAccount();
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _authServiceMock.Verify(
                x => x.DeactivateAccountAsync(It.IsAny<int>()),
                Times.Never);
        }
    }
}
