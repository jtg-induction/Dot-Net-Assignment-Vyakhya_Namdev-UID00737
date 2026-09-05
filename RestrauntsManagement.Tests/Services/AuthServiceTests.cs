using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Implementations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Tests.Services
{
    [TestClass]
    public class AuthServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private AuthService _authService;

        [TestInitialize]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            _authService = new AuthService(
                _userRepositoryMock.Object,
                _refreshTokenRepositoryMock.Object);
        }

        private SignupRequest GetValidSignupRequest()
        {
            return new SignupRequest
            {
                Name = "Vyakhya Namdev",
                Email = " VyakhyaNamdev@Test.com ",
                Password = "vyakhya@123",
                PhoneNumber = " 9876543210 ",
                HouseNumber = "12A",
                StreetAddress = "MG Road",
                City = "Noida",
                State = "Uttar Pradesh",
                PinCode = "110001",
                Country = "India",
                AddressType = AddressType.Home
            };
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

        [TestMethod]
        [Description("Verifies that Signup throws an ArgumentNullException when the signup request is null.")]
        public async Task Signup_NullRequest_ThrowsArgumentNullException()
        {
            Func<Task> action = () => _authService.Signup(null);
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        [Description("Verifies that Signup throws a conflict exception when the email is already registered.")]
        public async Task Signup_EmailAlreadyExists_ThrowsConflictException()
        {
            var request = GetValidSignupRequest();

            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync("vyakhyanamdev@test.com"))
                .ReturnsAsync(true);

            Func<Task> action = () => _authService.Signup(request);
            var exception = await action.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);

            _userRepositoryMock.Verify(
                x => x.EmailExistsAsync("vyakhyanamdev@test.com"),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that Signup throws a conflict exception when the phone number is already registered.")]
        public async Task Signup_PhoneNumberAlreadyExists_ThrowsConflictException()
        {
            var request = GetValidSignupRequest();
            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync("vyakhyanamdev@test.com"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsAsync("9876543210"))
                .ReturnsAsync(true);

            Func<Task> action = () => _authService.Signup(request);
            var exception = await action.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);

            _userRepositoryMock.Verify(
                x => x.PhoneNumberExistsAsync("9876543210"),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that Signup creates a customer account and returns the expected signup response for a valid request.")]
        public async Task Signup_ValidRequest_ReturnsSignupResponse()
        {
            var request = GetValidSignupRequest();
            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync("vyakhyanamdev@test.com"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsAsync("9876543210"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>()))
                .Callback<User>(user => user.Id = 1);

            var response = await _authService.Signup(request);
            response.Should().NotBeNull();
            response.UserId.Should().Be(1);
            response.Name.Should().Be("Vyakhya Namdev");
            response.Email.Should().Be("vyakhyanamdev@test.com");
            response.Role.Should().Be(UserRole.Customer.ToString());
            response.Balance.Should().Be(1000);

            _userRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that Signup normalizes the email and trims the phone number before saving the user.")]
        public async Task Signup_NormalizesUserDataBeforeSaving()
        {
            var request = GetValidSignupRequest();
            User savedUser = null;

            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync("vyakhyanamdev@test.com"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsAsync("9876543210"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>()))
                .Callback<User>(user =>
                {
                    savedUser = user;
                    user.Id = 1;
                });

            await _authService.Signup(request);
            savedUser.Should().NotBeNull();
            savedUser.Email.Should().Be("vyakhyanamdev@test.com");
            savedUser.PhoneNumber.Should().Be("9876543210");
            savedUser.Name.Should().Be("Vyakhya Namdev");
        }

        [TestMethod]
        [Description("Verifies that Signup hashes the password and does not store the plain-text password.")]
        public async Task Signup_ValidRequest_HashesPassword()
        {
            var request = GetValidSignupRequest();
            User savedUser = null;

            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>()))
                .Callback<User>(user =>
                {
                    savedUser = user;
                    user.Id = 1;
                });

            await _authService.Signup(request);
            savedUser.Should().NotBeNull();
            savedUser.Password.Should().NotBe(request.Password);
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                savedUser.Password).Should().BeTrue();
        }

        [TestMethod]
        [Description("Verifies that Signup assigns the Customer role and creates an active home address mapping for the new user.")]
        public async Task Signup_ValidRequest_CreatesCustomerAndUserAddress()
        {
            var request = GetValidSignupRequest();
            User savedUser = null;

            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>()))
                .Callback<User>(user =>
                {
                    savedUser = user;
                    user.Id = 1;
                });

            await _authService.Signup(request);
            savedUser.Should().NotBeNull();
            savedUser.Role.Should().Be(UserRole.Customer);
            savedUser.UserAddresses.Should().HaveCount(1);
            savedUser.UserAddresses.First().Address.AddressType.Should().Be(AddressType.Home);
        }

        [TestMethod]
        [Description("Verifies that Login returns access and refresh tokens for valid credentials and stores the refresh token.")]
        public async Task LoginAsync_ValidCredentials_ReturnsLoginResponse()
        {
            var user = CreateActiveUser();
            var request = new LoginRequest
            {
                Email = user.Email,
                Password = "vyakhya@123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(user.Email))
                .ReturnsAsync(user);

            _refreshTokenRepositoryMock
                .Setup(x => x.Add(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(token => token.Id = 10)
                .Returns(Task.CompletedTask);

            var result = await _authService.LoginAsync(request);
            result.Should().NotBeNull();
            result.Name.Should().Be(user.Name);
            result.Email.Should().Be(user.Email);
            result.AccessToken.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBeNullOrWhiteSpace();

            _refreshTokenRepositoryMock.Verify(
                x => x.Add(It.Is<RefreshToken>(
                    token =>
                        token.UserId == user.Id &&
                        token.Id == 10 &&
                        !string.IsNullOrWhiteSpace(token.Token))),
                Times.Once);

            _refreshTokenRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Exactly(2));
        }

        [TestMethod]
        [Description("Verifies that Login rejects the request when no user exists with the provided email.")]
        public async Task LoginAsync_UserNotFound_ThrowsInvalidCredentials()
        {
            var request = new LoginRequest
            {
                Email = "unknown@test.com",
                Password = "vyakhya@123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync("unknown@test.com"))
                .ReturnsAsync((User)null);

            Func<Task> action = () => _authService.LoginAsync(request);
            var exception = await action.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _refreshTokenRepositoryMock.Verify(
                x => x.Add(It.IsAny<RefreshToken>()),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that Login rejects the request when the provided password does not match the stored password.")]
        public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentials()
        {
            var user = CreateActiveUser();
            var request = new LoginRequest
            {
                Email = user.Email,
                Password = "WrongPassword"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(user.Email))
                .ReturnsAsync(user);

            Func<Task> action = () => _authService.LoginAsync(request);
            var exception = await action.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            _refreshTokenRepositoryMock.Verify(
                x => x.Add(It.IsAny<RefreshToken>()),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that Login generates a refresh token linked to the authenticated user.")]
        public async Task LoginAsync_ValidCredentials_CreatesRefreshTokenForUser()
        {
            var user = CreateActiveUser();
            var request = new LoginRequest
            {
                Email = user.Email,
                Password = "vyakhya@123"
            };

            RefreshToken savedToken = null;
            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(user.Email))
                .ReturnsAsync(user);

            _refreshTokenRepositoryMock
                .Setup(x => x.Add(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(token =>
                {
                    savedToken = token;
                    token.Id = 10;
                })
                .Returns(Task.CompletedTask);

            await _authService.LoginAsync(request);
            savedToken.Should().NotBeNull();
            savedToken.UserId.Should().Be(user.Id);
            savedToken.Token.Should().NotBeNullOrWhiteSpace();
            savedToken.Token.Should().HaveLength(64);
        }

        [TestMethod]
        [Description("Verifies that a valid refresh token generates a new access token and refresh token and removes the old token.")]
        public async Task RefreshTokenAsync_ValidToken_RotatesTokens()
        {
            var user = CreateActiveUser();
            const string refreshToken = "valid-refresh-token";
            var existingToken = CreateValidRefreshToken(user, refreshToken);

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(
                    DotNetRestaurantManagement.Helpers.TokenHelper.Hash(refreshToken)))
                .ReturnsAsync(existingToken);

            _refreshTokenRepositoryMock
                .Setup(x => x.Add(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(token => token.Id = 20)
                .Returns(Task.CompletedTask);

            var result = await _authService.RefreshTokenAsync(refreshToken);
            result.Should().NotBeNull();
            result.AccessToken.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBe(refreshToken);

            _refreshTokenRepositoryMock.Verify(
                x => x.Delete(existingToken),
                Times.Once);

            _refreshTokenRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Exactly(2));
        }

        [TestMethod]
        [Description("Verifies that RefreshToken rejects a token that does not exist in the database.")]
        public async Task RefreshTokenAsync_TokenNotFound_ThrowsInvalidCredentials()
        {
            const string refreshToken = "invalid-refresh-token";

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(
                    DotNetRestaurantManagement.Helpers.TokenHelper.Hash(refreshToken)))
                .ReturnsAsync((RefreshToken)null);

            Func<Task> action = () => _authService.RefreshTokenAsync(refreshToken);
            var exception = await action.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            _refreshTokenRepositoryMock.Verify(
                x => x.Delete(It.IsAny<RefreshToken>()),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that RefreshToken rejects an expired refresh token.")]
        public async Task RefreshTokenAsync_ExpiredToken_ThrowsInvalidCredentials()
        {
            var user = CreateActiveUser();
            const string refreshToken = "expired-refresh-token";
            var existingToken = CreateValidRefreshToken(user, refreshToken);
            existingToken.CreatedAt = DateTime.UtcNow.AddDays(-8);

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(
                    DotNetRestaurantManagement.Helpers.TokenHelper.Hash(refreshToken)))
                .ReturnsAsync(existingToken);

            Func<Task> action = () => _authService.RefreshTokenAsync(refreshToken);
            var exception = await action.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            _refreshTokenRepositoryMock.Verify(
                x => x.Add(It.IsAny<RefreshToken>()),
                Times.Never);

            _refreshTokenRepositoryMock.Verify(
                x => x.Delete(It.IsAny<RefreshToken>()),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that RefreshToken creates a new refresh token for the same user during token rotation.")]
        public async Task RefreshTokenAsync_ValidToken_CreatesNewRefreshToken()
        {
            var user = CreateActiveUser();
            const string refreshToken = "old-refresh-token";
            var existingToken = CreateValidRefreshToken(user, refreshToken);
            RefreshToken newToken = null;

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(
                    DotNetRestaurantManagement.Helpers.TokenHelper.Hash(refreshToken)))
                .ReturnsAsync(existingToken);

            _refreshTokenRepositoryMock
                .Setup(x => x.Add(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(token =>
                {
                    newToken = token;
                    token.Id = 20;
                })
                .Returns(Task.CompletedTask);

            await _authService.RefreshTokenAsync(refreshToken);

            newToken.Should().NotBeNull();
            newToken.UserId.Should().Be(user.Id);
            newToken.Token.Should().NotBeNullOrWhiteSpace();
            newToken.Token.Should().HaveLength(64);
            newToken.Token.Should().NotBe(existingToken.Token);
        }

        [TestMethod]
        [Description("Verifies that Logout deletes the refresh token associated with the authenticated user and refresh token ID.")]
        public async Task LogoutAsync_ValidUserAndRefreshToken_DeletesToken()
        {
            const long userId = 1;
            const long refreshTokenId = 10;

            var refreshToken = new RefreshToken
            {
                Id = refreshTokenId,
                UserId = userId,
                Token = "refresh-token-hash",
                CreatedAt = DateTime.UtcNow
            };

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByIdAsync(refreshTokenId))
                .ReturnsAsync(refreshToken);

            await _authService.LogoutAsync(userId, refreshTokenId);

            _refreshTokenRepositoryMock.Verify(
                x => x.GetByIdAsync(refreshTokenId),
                Times.Once);

            _refreshTokenRepositoryMock.Verify(
                x => x.Delete(refreshToken),
                Times.Once);

            _refreshTokenRepositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [TestMethod]
        [Description("Verifies that Logout rejects the request when the refresh token ID does not exist.")]
        public async Task LogoutAsync_RefreshTokenNotFound_ThrowsInvalidCredentials()
        {
            _refreshTokenRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync((RefreshToken)null);

            Func<Task> action = () => _authService.LogoutAsync(1, 10);

            var exception = await action.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            _refreshTokenRepositoryMock.Verify(
                x => x.Delete(It.IsAny<RefreshToken>()),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that Logout rejects an invalid user ID without querying the refresh token repository.")]
        public async Task LogoutAsync_InvalidUserId_ThrowsInvalidCredentials()
        {
            Func<Task> action = () => _authService.LogoutAsync(0, 10);

            var exception = await action.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            _refreshTokenRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<long>()),
                Times.Never);

            _refreshTokenRepositoryMock.Verify(
                x => x.Delete(It.IsAny<RefreshToken>()),
                Times.Never);
        }

        [TestMethod]
        [Description("Verifies that Logout rejects an invalid refresh token ID without querying the refresh token repository.")]
        public async Task LogoutAsync_InvalidRefreshTokenId_ThrowsInvalidCredentials()
        {
            Func<Task> action = () => _authService.LogoutAsync(1, 0);

            var exception = await action.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            _refreshTokenRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<long>()),
                Times.Never);

            _refreshTokenRepositoryMock.Verify(
                x => x.Delete(It.IsAny<RefreshToken>()),
                Times.Never);
        }
        
        [TestMethod]
        public async Task UpdateProfileAsync_ShouldUpdateProfileSuccessfully()
        {
            var user = new User
            {
                Id = 8,
                Name = "Vyakhya Namdev",
                Email = "vyakhya@gmail.com",
                PhoneNumber = "9876543210"
            };

            var request = new UpdateProfileRequest
            {
                Name = " Vyakhya Namdev ",
                Email = "VYAKHYA.NEW@GMAIL.COM",
                PhoneNumber = " 9081231239 "
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(8))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.EmailExistsForOtherUserAsync(
                    It.IsAny<string>(), 8))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsForOtherUserAsync(
                    It.IsAny<string>(), 8))
                .ReturnsAsync(false);

            var result = await _authService.UpdateProfileAsync(8, request);
            result.Should().NotBeNull();
            result.Id.Should().Be(8);
            result.Name.Should().Be("Vyakhya Namdev");
            result.Email.Should().Be("vyakhya.new@gmail.com");
            result.PhoneNumber.Should().Be("9081231239");
            _userRepositoryMock.Verify(
                x => x.UpdateProfileAsync(
                    8,
                    "Vyakhya Namdev",
                    "vyakhya.new@gmail.com",
                    "9081231239"),
                Times.Once);
            _userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_ShouldThrowUserNotFound_WhenUserDoesNotExist()
        {
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(8))
                .ReturnsAsync((User)null);
            var request = new UpdateProfileRequest
            {
                Name = "Vyakhya Namdev",
                Email = "vyakhya@gmail.com",
                PhoneNumber = "9876543210"
            };

            Func<Task> act = async () => await _authService.UpdateProfileAsync(8, request);
            await act.Should().ThrowAsync<UserNotFound>();
        }

        [TestMethod]
        public async Task UpdateProfileAsync_ShouldThrowDuplicateEmail_WhenEmailAlreadyExists()
        {
            var user = new User
            {
                Id = 8,
                Name = "Vyakhya Namdev",
                Email = "vyakhya@gmail.com",
                PhoneNumber = "9876543210"
            };

            var request = new UpdateProfileRequest
            {
                Name = "Vyakhya Namdev",
                Email = "another@gmail.com",
                PhoneNumber = "9876543210"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(8))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.EmailExistsForOtherUserAsync(
                    "another@gmail.com", 8))
                .ReturnsAsync(true);
            Func<Task> act = async () => await _authService.UpdateProfileAsync(8, request);
            await act.Should().ThrowAsync<DuplicateEmailException>();
            _userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [TestMethod]
        public async Task UpdateProfileAsync_ShouldThrowDuplicatePhone_WhenPhoneAlreadyExists()
        {
            var user = new User
            {
                Id = 8,
                Name = "Vyakhya Namdev",
                Email = "vyakhya@gmail.com",
                PhoneNumber = "9876543210"
            };
            var request = new UpdateProfileRequest
            {
                Name = "Vyakhya Namdev",
                Email = "new@gmail.com",
                PhoneNumber = "9999999999"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(8))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(x => x.EmailExistsForOtherUserAsync(
                    "new@gmail.com", 8))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsForOtherUserAsync(
                    "9999999999", 8))
                .ReturnsAsync(true);

            Func<Task> act = async () => await _authService.UpdateProfileAsync(8, request);
            await act.Should().ThrowAsync<PhoneNumberAlreadyRegistered>();
            _userRepositoryMock.Verify(
               x => x.SaveChanges(),
               Times.Never);
        }

        [TestMethod]
        public async Task ChangePasswordAsync_ShouldChangePasswordSuccessfully()
        {
            var user = new User
            {
                Id = 8,
                Name = "Vyakhya Namdev",
                Email = "vyakhya@gmail.com",
                Password = "old-hashed-password"
            };

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(8))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify(
                    "OldPassword123",
                    "old-hashed-password"))
                .Returns(true);

            _passwordHasherMock
                .Setup(x => x.Hash("NewPassword123"))
                .Returns("new-hashed-password");
            await _authService.ChangePasswordAsync(8, request);

            user.Password.Should().Be("new-hashed-password");

            _passwordHasherMock.Verify(
                x => x.Verify(
                    "OldPassword123",
                    "old-hashed-password"),
                Times.Once);

            _passwordHasherMock.Verify(
                x => x.Hash("NewPassword123"),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [TestMethod]
        public async Task ChangePasswordAsync_ShouldThrowInvalidCredentials_WhenCurrentPasswordIsWrong()
        {
            var user = new User
            {
                Id = 8,
                Name = "Vyakhya Namdev",
                Email = "vyakhya@gmail.com",
                Password = "old-hashed-password"
            };

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "WrongPassword",
                NewPassword = "NewPassword123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(8))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify(
                    "WrongPassword",
                    "old-hashed-password"))
                .Returns(false);

            Func<Task> act = async () => await _authService.ChangePasswordAsync(8, request);
            await act.Should().ThrowAsync<InvalidCredentialsException>();
            _passwordHasherMock.Verify(
                x => x.Hash(It.IsAny<string>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [TestMethod]
        public async Task ChangePasswordAsync_ShouldThrowUserNotFound_WhenUserDoesNotExist()
        {
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(8))
                .ReturnsAsync((User)null);

            var request = new ChangePasswordRequest
            {
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword123"
            };

            Func<Task> act = async () => await _authService.ChangePasswordAsync(8, request);
            await act.Should().ThrowAsync<UserNotFound>();
            _passwordHasherMock.Verify(
                x => x.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public async Task AddAddressAsync_ShouldAddAddressSuccessfully()
        {
            var user = new User
            {
                Id = 8,
                Name = "Vyakhya Namdev",
                Email = "vyakhya@gmail.com"
            };

            var request = new AddressRequest
            {
                HouseNumber = "123",
                StreetAddress = "MG Road",
                City = "Pune",
                State = "Maharashtra",
                PinCode = "411001",
                Country = "India",
                AddressType = (int)AddressType.Home
            };

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(8))
                .ReturnsAsync(user);
            _userRepositoryMock
                .Setup(x => x.AddAddress(It.IsAny<Address>()))
                .Callback<Address>(address =>
                {
                    address.Id = 100;
                });

            var result = await _authService.AddAddressAsync(8, request);
            result.Should().NotBeNull();
            result.Id.Should().Be(100);
            result.HouseNumber.Should().Be("123");
            result.StreetAddress.Should().Be("MG Road");
            result.City.Should().Be("Pune");
            result.State.Should().Be("Maharashtra");
            result.PinCode.Should().Be("411001");
            result.Country.Should().Be("India");

            _userRepositoryMock.Verify(
                x => x.AddAddress(It.Is<Address>(a =>
                    a.HouseNumber == "123" &&
                    a.StreetAddress == "MG Road" &&
                    a.City == "Pune" &&
                    a.State == "Maharashtra" &&
                    a.PinCode == "411001" &&
                    a.Country == "India")),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.AddUserAddress(It.Is<UserAddress>(ua =>
                    ua.UserId == 8 &&
                    ua.AddressId == 100)),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Exactly(2));
        }

        [TestMethod]
        public async Task AddAddressAsync_ShouldThrowUserNotFound_WhenUserDoesNotExist()
        {
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(8))
                .ReturnsAsync((User)null);

            var request = new AddressRequest
            {
                HouseNumber = "123",
                StreetAddress = "MG Road",
                City = "Pune",
                State = "Maharashtra",
                PinCode = "411001",
                Country = "India",
                AddressType = (int)AddressType.Home
            };

            Func<Task> act = async () => await _authService.AddAddressAsync(8, request);
            await act.Should().ThrowAsync<UserNotFound>();
            _userRepositoryMock.Verify(
                x => x.AddAddress(It.IsAny<Address>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [TestMethod]
        public async Task AddAddressAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
        {
            Func<Task> act = async () => await _authService.AddAddressAsync(8, null);
            await act.Should().ThrowAsync<ArgumentNullException>();

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<int>()),
                Times.Never);
        }
    }
}
