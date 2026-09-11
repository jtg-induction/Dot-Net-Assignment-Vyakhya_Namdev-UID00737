using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Implementations;
using DotNetRestaurantManagement.Services.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Tests.Services
{
    [TestClass]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private UserService _userService;

        [TestInitialize]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            _userService = new UserService(
                _userRepositoryMock.Object,
                _refreshTokenRepositoryMock.Object);
        }

        private User CreateActiveUser()
        {
            return new User
            {
                Id = 1,
                Name = "Vyakhya Namdev",
                Email = "vyakhyanamdev@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword("vyakhya@123"),
                PhoneNumber = "9876543210",
                IsActive = true,
                Role = UserRole.Customer
            };
        }

        private User CreateInactiveUser()
        {
            return new User
            {
                Id = 1,
                Name = "Vyakhya Namdev",
                Email = "vyakhyanamdev@test.com",
                Password = "hashed-password",
                IsActive = false
            };
        }

        private RefreshToken CreateValidRefreshToken(User user, string refreshToken)
        {
            return new RefreshToken
            {
                Id = 10,
                UserId = user.Id,
                Token = DotNetRestaurantManagement.Helpers.TokenHelper.Hash(refreshToken),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                User = user
            };
        }

        private AddressRequest GetValidAddressRequest()
        {
            return new AddressRequest
            {
                HouseNumber = "12A",
                StreetAddress = "MG Road",
                City = "Noida",
                State = "Uttar Pradesh",
                PinCode = "201301",
                Country = "India",
                AddressType = (int)AddressType.Home
            };
        }

        [TestMethod]
        [Description("Verifies that UpdateProfileAsync successfully updates both name and phone number.")]
        public async Task UpdateProfileAsync_ValidRequest_ReturnsUpdatedProfile()
        {
            var user = CreateActiveUser();

            var request = new UpdateProfileRequest
            {
                Name = "Updated Name",
                PhoneNumber = "9999999999"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsForOtherUserAsync("9999999999", 1))
                .ReturnsAsync(false);

            var result = await _userService.UpdateProfileAsync(1, request);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("Updated Name");
            result.PhoneNumber.Should().Be("9999999999");

            user.Name.Should().Be("Updated Name");
            user.PhoneNumber.Should().Be("9999999999");

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that UpdateProfileAsync updates only the name when phone number is not provided.")]
        public async Task UpdateProfileAsync_OnlyNameProvided_UpdatesOnlyName()
        {
            var user = CreateActiveUser();

            var request = new UpdateProfileRequest
            {
                Name = "Updated Name"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            var result = await _userService.UpdateProfileAsync(1, request);

            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Name");
            result.PhoneNumber.Should().Be("9876543210");

            user.Name.Should().Be("Updated Name");
            user.PhoneNumber.Should().Be("9876543210");

            _userRepositoryMock.Verify(
                x => x.PhoneNumberExistsForOtherUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<long>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that UpdateProfileAsync updates only the phone number when name is not provided.")]
        public async Task UpdateProfileAsync_OnlyPhoneNumberProvided_UpdatesOnlyPhoneNumber()
        {
            var user = CreateActiveUser();

            var request = new UpdateProfileRequest
            {
                PhoneNumber = "9999999999"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsForOtherUserAsync("9999999999", 1))
                .ReturnsAsync(false);

            var result = await _userService.UpdateProfileAsync(1, request);

            result.Should().NotBeNull();
            result.Name.Should().Be("Vyakhya Namdev");
            result.PhoneNumber.Should().Be("9999999999");

            user.Name.Should().Be("Vyakhya Namdev");
            user.PhoneNumber.Should().Be("9999999999");

            _userRepositoryMock.Verify(
                x => x.PhoneNumberExistsForOtherUserAsync(
                    "9999999999",
                    1),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that UpdateProfileAsync trims name and phone number before updating.")]
        public async Task UpdateProfileAsync_NormalizesValues()
        {
            var user = CreateActiveUser();

            var request = new UpdateProfileRequest
            {
                Name = "  Updated Name  ",
                PhoneNumber = " 9999999999 "
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsForOtherUserAsync("9999999999", 1))
                .ReturnsAsync(false);

            var result = await _userService.UpdateProfileAsync(1, request);

            result.Name.Should().Be("Updated Name");
            result.PhoneNumber.Should().Be("9999999999");
        }

        [TestMethod]
        [Description("Verifies that UpdateProfileAsync throws ArgumentNullException when the request is null.")]
        public async Task UpdateProfileAsync_NullRequest_ThrowsArgumentNullException()
        {
            Func<Task> action = () =>
                _userService.UpdateProfileAsync(1, null);

            await action.Should().ThrowAsync<ArgumentNullException>();

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<long>()),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that UpdateProfileAsync throws Unauthorized when the user does not exist.")]
        public async Task UpdateProfileAsync_UserDoesNotExist_ThrowsUnauthorized()
        {
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((User)null);

            var request = new UpdateProfileRequest
            {
                Name = "Updated Name"
            };

            Func<Task> action = () =>
                _userService.UpdateProfileAsync(1, request);

            var exception =
                await action.Should().ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.Unauthorized);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that UpdateProfileAsync throws Unauthorized when the user is inactive.")]
        public async Task UpdateProfileAsync_InactiveUser_ThrowsUnauthorized()
        {
            var user = CreateActiveUser();
            user.IsActive = false;

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            var request = new UpdateProfileRequest
            {
                Name = "Updated Name"
            };

            Func<Task> action = () =>
                _userService.UpdateProfileAsync(1, request);

            var exception =
                await action.Should().ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.Unauthorized);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that UpdateProfileAsync throws Conflict when the new phone number belongs to another user.")]
        public async Task UpdateProfileAsync_PhoneNumberAlreadyExists_ThrowsConflict()
        {
            var user = CreateActiveUser();

            var request = new UpdateProfileRequest
            {
                PhoneNumber = "9999999999"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsForOtherUserAsync(
                    "9999999999",
                    1))
                .ReturnsAsync(true);

            Func<Task> action = () =>
                _userService.UpdateProfileAsync(1, request);

            var exception =
                await action.Should().ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.Conflict);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that UpdateProfileAsync does not check phone duplication when the phone number is unchanged.")]
        public async Task UpdateProfileAsync_SamePhoneNumber_DoesNotCheckDuplication()
        {
            var user = CreateActiveUser();

            var request = new UpdateProfileRequest
            {
                Name = "Updated Name",
                PhoneNumber = "9876543210"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            var result =
                await _userService.UpdateProfileAsync(1, request);

            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Name");
            result.PhoneNumber.Should().Be("9876543210");

            _userRepositoryMock.Verify(
                x => x.PhoneNumberExistsForOtherUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<long>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that ChangePasswordAsync changes the password when the current password is correct.")]
        public async Task ChangePasswordAsync_ValidRequest_ChangesPassword()
        {
            var user = CreateActiveUser();
            string oldPasswordHash = user.Password;

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "vyakhya@123",
                NewPassword = "NewPassword@123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            await _userService.ChangePasswordAsync(1, request);

            user.Password.Should().NotBe(oldPasswordHash);

            BCrypt.Net.BCrypt.Verify(
                "NewPassword@123",
                user.Password).Should().BeTrue();

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that ChangePasswordAsync throws Unauthorized when the user does not exist.")]
        public async Task ChangePasswordAsync_UserDoesNotExist_ThrowsUnauthorized()
        {
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((User)null);

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "Current@123",
                NewPassword = "NewPassword@123"
            };

            Func<Task> action = () =>
                _userService.ChangePasswordAsync(1, request);

            var exception =
                await action.Should().ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.Unauthorized);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that ChangePasswordAsync throws Unauthorized when the current password is incorrect.")]
        public async Task ChangePasswordAsync_IncorrectCurrentPassword_ThrowsUnauthorized()
        {
            var user = CreateActiveUser();

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "WrongPassword@123",
                NewPassword = "NewPassword@123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            Func<Task> action = () =>
                _userService.ChangePasswordAsync(1, request);

            var exception =
                await action.Should().ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.Unauthorized);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that ChangePasswordAsync throws NoContent when the new password is the same as the current password.")]
        public async Task ChangePasswordAsync_NewPasswordSameAsCurrent_ThrowsNoContent()
        {
            var user = CreateActiveUser();

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "vyakhya@123",
                NewPassword = "vyakhya@123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            Func<Task> action = () =>
                _userService.ChangePasswordAsync(1, request);

            var exception =
                await action.Should().ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.NoContent);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that AddAddressAsync successfully creates and associates an address with the user.")]
        public async Task AddAddressAsync_ValidRequest_ReturnsAddressResponse()
        {
            var user = CreateActiveUser();
            var request = GetValidAddressRequest();

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.GetUserAddressAsync(1))
                .ReturnsAsync((UserAddress)null);

            _userRepositoryMock
                .Setup(x => x.AddUserAddressAsync(
                    It.IsAny<Address>(),
                    It.IsAny<UserAddress>()))
                .Callback<Address, UserAddress>((address, userAddress) =>
                {
                    address.Id = 10;
                })
                .Returns(Task.CompletedTask);

            var result =
                await _userService.AddAddressAsync(1, request);

            result.Should().NotBeNull();
            result.Id.Should().Be(10);
            result.HouseNumber.Should().Be("12A");
            result.StreetAddress.Should().Be("MG Road");
            result.City.Should().Be("Noida");
            result.State.Should().Be("Uttar Pradesh");
            result.PinCode.Should().Be("201301");
            result.Country.Should().Be("India");
            result.AddressType.Should().Be((int)AddressType.Home);

            _userRepositoryMock.Verify(
                x => x.AddUserAddressAsync(
                    It.IsAny<Address>(),
                    It.IsAny<UserAddress>()),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.AddUserAddressAsync(
                    It.Is<Address>(a =>
                        a.HouseNumber == "12A" &&
                        a.StreetAddress == "MG Road" &&
                        a.City == "Noida"),
                    It.Is<UserAddress>(ua =>
                        ua.UserId == 1 &&
                        ua.IsActive)),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that AddAddressAsync trims address fields before creating the address.")]
        public async Task AddAddressAsync_RequestContainsSpaces_TrimsAddressFields()
        {
            var user = CreateActiveUser();

            var request = new AddressRequest
            {
                HouseNumber = " 12A ",
                StreetAddress = " MG Road ",
                City = " Noida ",
                State = " Uttar Pradesh ",
                PinCode = " 201301 ",
                Country = " India ",
                AddressType = (int)AddressType.Home
            };

            Address addedAddress = null;

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.GetUserAddressAsync(1))
                .ReturnsAsync((UserAddress)null);

            _userRepositoryMock
                .Setup(x => x.AddUserAddressAsync(
                    It.IsAny<Address>(),
                    It.IsAny<UserAddress>()))
                .Callback<Address, UserAddress>((address, userAddress) =>
                {
                    addedAddress = address;
                    address.Id = 10;
                })
                .Returns(Task.CompletedTask);

            await _userService.AddAddressAsync(1, request);

            addedAddress.Should().NotBeNull();
            addedAddress.HouseNumber.Should().Be("12A");
            addedAddress.StreetAddress.Should().Be("MG Road");
            addedAddress.City.Should().Be("Noida");
            addedAddress.State.Should().Be("Uttar Pradesh");
            addedAddress.PinCode.Should().Be("201301");
            addedAddress.Country.Should().Be("India");
        }

        [TestMethod]
        [Description("Verifies that AddAddressAsync deactivates the existing active address before adding a new address.")]
        public async Task AddAddressAsync_ExistingAddress_DeactivatesPreviousAddress()
        {
            var user = CreateActiveUser();

            var currentAddress = new UserAddress
            {
                AddressId = 5,
                UserId = 1,
                IsActive = true
            };

            var request = GetValidAddressRequest();

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.GetUserAddressAsync(1))
                .ReturnsAsync(currentAddress);

            _userRepositoryMock
                .Setup(x => x.AddUserAddressAsync(
                    It.IsAny<Address>(),
                    It.IsAny<UserAddress>()))
                .Callback<Address, UserAddress>((address, userAddress) =>
                {
                    address.Id = 10;
                })
                .Returns(Task.CompletedTask);

            await _userService.AddAddressAsync(1, request);

            currentAddress.IsActive.Should().BeFalse();
        }

        [TestMethod]
        [Description("Verifies that AddAddressAsync throws Unauthorized when the user does not exist.")]
        public async Task AddAddressAsync_UserDoesNotExist_ThrowsUnauthorized()
        {
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((User)null);

            var request = GetValidAddressRequest();

            Func<Task> action = () =>
                _userService.AddAddressAsync(1, request);

            var exception =
                await action.Should().ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.Unauthorized);

            _userRepositoryMock.Verify(
                x => x.AddUserAddressAsync(
                    It.IsAny<Address>(),
                    It.IsAny<UserAddress>()),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that AddAddressAsync throws Unauthorized when the user is inactive.")]
        public async Task AddAddressAsync_InactiveUser_ThrowsUnauthorized()
        {
            var user = CreateActiveUser();
            user.IsActive = false;

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            var request = GetValidAddressRequest();

            Func<Task> action = () =>
                _userService.AddAddressAsync(1, request);

            var exception =
                await action.Should().ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.Unauthorized);

            _userRepositoryMock.Verify(
                x => x.AddUserAddressAsync(
                    It.IsAny<Address>(),
                    It.IsAny<UserAddress>()),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that DeactivateAccountAsync throws Unauthorized when the user does not exist")]
        public async Task DeactivateAccountAsync_UserDoesNotExist_ThrowsApiException()
        {
            long userId = 1;
            long refreshTokenId = 10;

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync((int)userId))
                .ReturnsAsync((User)null);

            Func<Task> act = async () =>
                await _userService.DeactivateAccountAsync(userId, refreshTokenId);

            var exception = await act.Should().ThrowAsync<ApiException>();

            exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            exception.Which.Message.Should().Be(ErrorMessages.UserNotFound);

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync((int)userId),
                Times.Once);

            _refreshTokenRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<long>()),
                Times.Never);

            _refreshTokenRepositoryMock.Verify(
                x => x.Delete(It.IsAny<RefreshToken>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that DeactivateAccountAsync throws Unauthorized when the user is already inactive")]
        public async Task DeactivateAccountAsync_UserIsInactive_ThrowsApiException()
        {
            long userId = 1;
            long refreshTokenId = 10;

            var user = CreateInactiveUser();

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync((int)userId))
                .ReturnsAsync(user);

            Func<Task> act = async () =>
                await _userService.DeactivateAccountAsync(userId, refreshTokenId);

            var exception = await act.Should().ThrowAsync<ApiException>();

            exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            exception.Which.Message.Should().Be(ErrorMessages.UserNotFound);

            _refreshTokenRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<long>()),
                Times.Never);

            _refreshTokenRepositoryMock.Verify(
                x => x.Delete(It.IsAny<RefreshToken>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that DeactivateAccountAsync throws BadRequest when the refresh token ID is invalid")]
        public async Task DeactivateAccountAsync_InvalidRefreshTokenId_ThrowsApiException()
        {
            long userId = 1;
            long refreshTokenId = 0;

            var user = CreateActiveUser();

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync((int)userId))
                .ReturnsAsync(user);

            Func<Task> act = async () =>
                await _userService.DeactivateAccountAsync(userId, refreshTokenId);

            var exception = await act.Should().ThrowAsync<ApiException>();

            exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Which.Message.Should().Be(ErrorMessages.InvalidRefreshToken);

            user.IsActive.Should().BeFalse();

            _refreshTokenRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<long>()),
                Times.Never);

            _refreshTokenRepositoryMock.Verify(
                x => x.Delete(It.IsAny<RefreshToken>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that DeactivateAccountAsync throws ApiException when the refresh token belongs to another user.")]
        public async Task DeactivateAccountAsync_RefreshTokenBelongsToAnotherUser_ThrowsApiException()
        {
            long userId = 1;
            long refreshTokenId = 10;

            var user = CreateActiveUser();
            var refreshToken = CreateValidRefreshToken(user, "refresh-token");

            refreshToken.UserId = 2;

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync((int)userId))
                .ReturnsAsync(user);

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByIdAsync(refreshTokenId))
                .ReturnsAsync(refreshToken);

            Func<Task> act = async () =>
                await _userService.DeactivateAccountAsync(userId, refreshTokenId);

            var exception = await act.Should().ThrowAsync<ApiException>();

            exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            exception.Which.Message.Should().Be(ErrorMessages.InvalidRefreshToken);

            user.IsActive.Should().BeFalse();

            _refreshTokenRepositoryMock.Verify(
                x => x.GetByIdAsync(refreshTokenId),
                Times.Once);

            _refreshTokenRepositoryMock.Verify(
                x => x.Delete(It.IsAny<RefreshToken>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that DeactivateAccountAsync deactivates the user and deletes the valid refresh token")]
        public async Task DeactivateAccountAsync_ValidRequest_DeactivatesUserAndDeletesRefreshToken()
        {
            long userId = 1;
            long refreshTokenId = 10;

            var user = CreateActiveUser();
            var refreshToken = CreateValidRefreshToken(user, "refresh-token");

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync((int)userId))
                .ReturnsAsync(user);

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByIdAsync(refreshTokenId))
                .ReturnsAsync(refreshToken);

            await _userService.DeactivateAccountAsync(userId, refreshTokenId);

            user.IsActive.Should().BeFalse();

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync((int)userId),
                Times.Once);

            _refreshTokenRepositoryMock.Verify(
                x => x.GetByIdAsync(refreshTokenId),
                Times.Once);

            _refreshTokenRepositoryMock.Verify(
                x => x.Delete(refreshToken),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }
    }
}
