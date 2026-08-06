using DotNetRestaurantManagement.Controllers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Services.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
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

        /// <summary>
        /// Verifies that a valid signup request returns HTTP 201 Created with the expected user details.
        /// </summary>
        [TestMethod]
        public async Task Signup_ValidRequest_ReturnsCreated()
        {
            SignupRequest request = GetValidRequest();
            SignupResponse response = GetValidResponse();
            _authServiceMock
                .Setup(x => x.Signup(It.IsAny<SignupRequest>()))
                .ReturnsAsync(response);
            var result = await _controller.Signup(request);
            var createdResult = result as NegotiatedContentResult<SignupResponse>;
            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(HttpStatusCode.Created);
            createdResult.Content.Should().NotBeNull();
            createdResult.Content.UserId.Should().Be(1);
            createdResult.Content.Name.Should().Be("Vyakhya Namdev");
            createdResult.Content.Email.Should().Be("vyakhyanamdev@test.com");
            createdResult.Content.Role.Should().Be("Customer");
            createdResult.Content.Balance.Should().Be(1000);
        }

        /// <summary>
        /// Verifies that the signup service is called exactly once for a valid signup request.
        /// </summary>
        [TestMethod]
        public async Task Signup_ValidRequest_CallsServiceExactlyOnce()
        {
            SignupRequest request = GetValidRequest();
            SignupResponse response = GetValidResponse();
            _authServiceMock
                .Setup(x => x.Signup(It.IsAny<SignupRequest>()))
                .ReturnsAsync(response);
            await _controller.Signup(request);
            _authServiceMock.Verify(
                x => x.Signup(It.IsAny<SignupRequest>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that a null signup request is passed to the signup service and the controller returns the service response.
        /// </summary>
        [TestMethod]
        public async Task Signup_NullRequest_PassesRequestToService()
        {
            SignupRequest request = null;
            SignupResponse response = GetValidResponse();
            _authServiceMock
                .Setup(x => x.Signup(null))
                .ReturnsAsync(response);
            var result = await _controller.Signup(request);
            var createdResult = result as NegotiatedContentResult<SignupResponse>;
            createdResult.Should().NotBeNull();
            createdResult.StatusCode.Should().Be(HttpStatusCode.Created);
            _authServiceMock.Verify(
                x => x.Signup(null),
                Times.Once);
        }

        /// <summary>
        /// Verifies that an exception thrown by the signup service is propagated by the controller.
        /// </summary>
        [TestMethod]
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

        /// <summary>
        /// Verifies that the controller passes the correct signup request data to the signup service.
        /// </summary>
        [TestMethod]
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
            )
        }
        
        /// <summary>
        /// Verifies that the controller constructor throws an ArgumentNullException when the auth service is null.
        /// </summary>
        [TestMethod]
        public void Constructor_NullAuthService_ThrowsArgumentNullException()
        {
            Action act = () => new AuthController(null);
            act.Should().Throw<ArgumentNullException>();
        }
        
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            LoginRequest request = new LoginRequest
            {
                Email = "vyakhya@test.com",
                Password = "WrongPassword"
            };

            _authServiceMock
                .Setup(x => x.LoginAsync(request))
                .ThrowsAsync(new InvalidCredentialsException());
            IHttpActionResult result = await _controller.Login(request);
            result.Should().BeOfType<UnauthorizedResult>();
            _authServiceMock.Verify(
                x => x.LoginAsync(request),
                Times.Once);
        }

        [TestMethod]
        public async Task Login_WhenServiceThrowsException_ShouldReturnInternalServerError()
        {
            LoginRequest request = new LoginRequest
            {
                Email = "vyakhya@test.com",
                Password = "Vyakhya@123"
            };

            _authServiceMock
                .Setup(x => x.LoginAsync(request))
                .ThrowsAsync(
                    new System.Exception("Database error"));

            IHttpActionResult result = await _controller.Login(request);
            result.Should().BeOfType<InternalServerErrorResult>();
            _authServiceMock.Verify(
                x => x.LoginAsync(request),
                Times.Once);
        }

        [TestMethod]
        public async Task Login_ShouldPassCorrectRequestToService()
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
                Email = "vyakhya@test.com",
            };

            _authServiceMock
                .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(response);

            await _controller.Login(request);
            _authServiceMock.Verify(
                service => service.LoginAsync(
                    It.Is<LoginRequest>(req =>
                        req.Email == "vyakhya@test.com" &&
                        req.Password == "Vyakhya@123")),
                Times.Once);
        }

        [TestMethod]
        public async Task Login_WithNullRequest_ShouldNotCallService()
        {
            IHttpActionResult result = await _controller.Login(null);
            result.Should().NotBeNull();
            _authServiceMock.Verify(
                x => x.LoginAsync(
                    It.IsAny<LoginRequest>()),
                Times.Never);
        }
    }
}
