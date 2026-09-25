using DotNetRestaurantManagement.Constants;
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
    public class AddressControllerTests
    {
        private Mock<IAddressService> _addressServiceMock;
        private AddressController _controller;
        [TestInitialize]
        public void Setup()
        {
            _addressServiceMock = new Mock<IAddressService>();
            _controller = new AddressController(_addressServiceMock.Object);
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

            _addressServiceMock
                .Setup(x => x.AddAddressAsync(1, request))
                .ReturnsAsync(response);

            var result = await _controller.AddAddress(request);

            var createdResult =
                result.Should()
                    .BeOfType<OkNegotiatedContentResult<ApiResponse<object>>>()
                    .Subject;

            createdResult.Content.Success.Should().BeTrue();
            createdResult.Content.Message.Should().Be(SuccessMessages.AddressAdded);
            createdResult.Content.Data.Should().Be(response);

            _addressServiceMock.Verify(
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

            _addressServiceMock
                .Setup(x => x.AddAddressAsync(
                    It.IsAny<long>(),
                    It.IsAny<AddressRequest>()))
                .ReturnsAsync(response);

            await _controller.AddAddress(request);

            _addressServiceMock.Verify(
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

            _addressServiceMock.Verify(
                x => x.AddAddressAsync(
                    It.IsAny<long>(),
                    It.IsAny<AddressRequest>()),
                Times.Never);
        }
    }
}
