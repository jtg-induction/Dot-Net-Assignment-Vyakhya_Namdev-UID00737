using DotNetRestaurantManagement.Controllers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Services.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace RestrauntsManagement.Tests.Controllers
{
    [TestClass]
    public class UserControllerTests
    {
        private Mock<IUserService> _userServiceMock;
        private UserController _controller;
        [TestInitialize]
        public void Setup()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UserController(_userServiceMock.Object);
        }

        private void SetUserClaims(long userId)
        {
            var claims = new[]
            {
            new Claim("userId", userId.ToString()),
            new Claim("refreshTokenId", "10"),
            new Claim("role", "Customer")
        };

            _controller.User =
                new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "TestAuth"));
        }

        private void SetUserWithoutUserIdClaim()
        {
            var claims = new[]
            {
            new Claim("refreshTokenId", "10"),
            new Claim("role", "Customer")
        };

            _controller.User =
                new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "TestAuth"));
        }

        [TestMethod]
        public async Task UpdateProfile_ValidRequest_ReturnsOk()
        {
            SetUserClaims(1);

            var request = new UpdateProfileRequest
            {
                Name = "Updated Name",
                PhoneNumber = "9999999999"
            };

            var response = new UpdateProfileResponse
            {
                Id = 1,
                Name = "Updated Name",
                PhoneNumber = "9999999999"
            };

            _userServiceMock
                .Setup(x => x.UpdateProfileAsync(1, request))
                .ReturnsAsync(response);

            var result = await _controller.UpdateProfile(request);

            var okResult =
                result.Should()
                    .BeOfType<OkNegotiatedContentResult<ApiResponse<UpdateProfileResponse>>>()
                    .Subject;

            okResult.Content.Success.Should().BeTrue();
            okResult.Content.Data.Should().Be(response);

            _userServiceMock.Verify(
                x => x.UpdateProfileAsync(1, request),
                Times.Once);
        }

        [TestMethod]
        public async Task UpdateProfile_ValidClaims_PassesCorrectUserIdAndRequest()
        {
            SetUserClaims(5);

            var request = new UpdateProfileRequest
            {
                Name = "Test User",
                PhoneNumber = "9876543210"
            };

            var response = new UpdateProfileResponse
            {
                Id = 5,
                Name = "Test User",
                PhoneNumber = "9876543210"
            };

            _userServiceMock
                .Setup(x => x.UpdateProfileAsync(It.IsAny<long>(), It.IsAny<UpdateProfileRequest>()))
                .ReturnsAsync(response);

            await _controller.UpdateProfile(request);

            _userServiceMock.Verify(
                x => x.UpdateProfileAsync(
                    5,
                    It.Is<UpdateProfileRequest>(r =>
                        r.Name == "Test User" &&
                        r.PhoneNumber == "9876543210")),
                Times.Once);
        }

        [TestMethod]
        public async Task UpdateProfile_MissingUserIdClaim_ThrowsUnauthorizedAccessException()
        {
            SetUserWithoutUserIdClaim();

            var request = new UpdateProfileRequest
            {
                Name = "Updated Name"
            };

            Func<Task> action = () => _controller.UpdateProfile(request);

            await action.Should().ThrowAsync<UnauthorizedAccessException>();

            _userServiceMock.Verify(
                x => x.UpdateProfileAsync(
                    It.IsAny<long>(),
                    It.IsAny<UpdateProfileRequest>()),
                Times.Never);
        }

        [TestMethod]
        public async Task ChangePassword_ValidRequest_ReturnsOk()
        {
            SetUserClaims(1);

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "OldPassword@123",
                NewPassword = "NewPassword@123"
            };

            _userServiceMock
                .Setup(x => x.ChangePasswordAsync(1, request))
                .Returns(Task.CompletedTask);

            var result = await _controller.ChangePassword(request);

            var okResult =
                result.Should()
                    .BeOfType<OkNegotiatedContentResult<bool>>()
                    .Subject;

            okResult.Content.Should().BeTrue();

            _userServiceMock.Verify(
                x => x.ChangePasswordAsync(1, request),
                Times.Once);
        }

        [TestMethod]
        public async Task ChangePassword_ValidClaims_PassesCorrectUserIdAndRequest()
        {
            SetUserClaims(5);

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "OldPassword@123",
                NewPassword = "NewPassword@123"
            };

            _userServiceMock
                .Setup(x => x.ChangePasswordAsync(
                    It.IsAny<long>(),
                    It.IsAny<ChangePasswordRequest>()))
                .Returns(Task.CompletedTask);

            await _controller.ChangePassword(request);

            _userServiceMock.Verify(
                x => x.ChangePasswordAsync(
                    5,
                    It.Is<ChangePasswordRequest>(r =>
                        r.CurrentPassword == "OldPassword@123" &&
                        r.NewPassword == "NewPassword@123")),
                Times.Once);
        }

        [TestMethod]
        public async Task ChangePassword_MissingUserIdClaim_ThrowsUnauthorizedAccessException()
        {
            SetUserWithoutUserIdClaim();

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "OldPassword@123",
                NewPassword = "NewPassword@123"
            };

            Func<Task> action = () => _controller.ChangePassword(request);

            await action.Should().ThrowAsync<UnauthorizedAccessException>();

            _userServiceMock.Verify(
                x => x.ChangePasswordAsync(
                    It.IsAny<long>(),
                    It.IsAny<ChangePasswordRequest>()),
                Times.Never);
        }

        [TestMethod]
        public async Task AddAddress_ValidRequest_ReturnsCreated()
        {
            SetUserClaims(1);

            var request = new AddressRequest
            {
                HouseNumber = "12A",
                StreetAddress = "MG Road",
                City = "Noida",
                State = "Uttar Pradesh",
                PinCode = "201301",
                Country = "India",
                AddressType = 1
            };

            var response = new AddressResponse
            {
                Id = 10,
                HouseNumber = "12A",
                StreetAddress = "MG Road",
                City = "Noida",
                State = "Uttar Pradesh",
                PinCode = "201301",
                Country = "India",
                AddressType = 1
            };

            _userServiceMock
                .Setup(x => x.AddAddressAsync(1, request))
                .ReturnsAsync(response);

            var result = await _controller.AddAddress(request);

            var createdResult =
                result.Should()
                    .BeOfType<NegotiatedContentResult<AddressResponse>>()
                    .Subject;

            createdResult.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            createdResult.Content.Should().Be(response);

            _userServiceMock.Verify(
                x => x.AddAddressAsync(1, request),
                Times.Once);
        }

        [TestMethod]
        public async Task AddAddress_ValidClaims_PassesCorrectUserIdAndRequest()
        {
            SetUserClaims(7);

            var request = new AddressRequest
            {
                HouseNumber = "12A",
                StreetAddress = "MG Road",
                City = "Noida",
                State = "Uttar Pradesh",
                PinCode = "201301",
                Country = "India",
                AddressType = 1
            };

            var response = new AddressResponse
            {
                Id = 10,
                HouseNumber = "12A",
                StreetAddress = "MG Road",
                City = "Noida",
                State = "Uttar Pradesh",
                PinCode = "201301",
                Country = "India",
                AddressType = 1
            };

            _userServiceMock
                .Setup(x => x.AddAddressAsync(
                    It.IsAny<long>(),
                    It.IsAny<AddressRequest>()))
                .ReturnsAsync(response);

            await _controller.AddAddress(request);

            _userServiceMock.Verify(
                x => x.AddAddressAsync(
                    7,
                    It.Is<AddressRequest>(r =>
                        r.HouseNumber == "12A" &&
                        r.StreetAddress == "MG Road" &&
                        r.City == "Noida" &&
                        r.State == "Uttar Pradesh" &&
                        r.PinCode == "201301" &&
                        r.Country == "India" &&
                        r.AddressType == 1)),
                Times.Once);
        }

        [TestMethod]
        public async Task AddAddress_MissingUserIdClaim_ThrowsUnauthorizedAccessException()
        {
            SetUserWithoutUserIdClaim();

            var request = new AddressRequest
            {
                HouseNumber = "12A",
                StreetAddress = "MG Road",
                City = "Noida",
                State = "Uttar Pradesh",
                PinCode = "201301",
                Country = "India",
                AddressType = 1
            };

            Func<Task> action = () => _controller.AddAddress(request);

            await action.Should().ThrowAsync<UnauthorizedAccessException>();

            _userServiceMock.Verify(
                x => x.AddAddressAsync(
                    It.IsAny<long>(),
                    It.IsAny<AddressRequest>()),
                Times.Never);
        }
    }
}
