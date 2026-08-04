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
    public class SignupControllerTests
    {
        private Mock<IAuthService> _authServiceMock;
        private AuthController _controller;

        [TestInitialize]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
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
        public void Constructor_NullAuthService_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new AuthController(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}