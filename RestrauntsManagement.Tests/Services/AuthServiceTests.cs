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
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Tests.Services
{
    [TestClass]
    public class AuthServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IPasswordHasher> _passwordHasherMock;
        private Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private Mock<IJwtService> _jwtServiceMock;
        private AuthService _authService;

        [TestInitialize]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            _jwtServiceMock = new Mock<IJwtService>();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _refreshTokenRepositoryMock.Object,
                _jwtServiceMock.Object);
        }

        private SignupRequest GetValidRequest()
        {
            return new SignupRequest
            {
                Name = "Vyakhya Namdev",
                Email = "vyakhyanamdev@test.com",
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
                Email = "vyakhyanamdev@test.com",
                Password = BCrypt.Net.BCrypt.HashPassword("vyakhya@123"),
                IsActive = true,
                Role = UserRole.Customer
            };
        }

        private RefreshToken CreateValidRefreshToken(User user)
        {
            return new RefreshToken
            {
                Token = "old-refresh-token",
                UserId = user.Id,
                User = user
            };
        }

        [TestMethod]
        public async Task Signup_NullRequest_ThrowsArgumentNullException()
        {
            Func<Task> action = () => _authService.Signup(null);
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task Signup_EmailAlreadyExists_ThrowsDuplicateEmailException()
        {
            var request = GetValidRequest();
            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            Func<Task> action = () => _authService.Signup(request);
            await action.Should().ThrowAsync<DuplicateEmailException>().WithMessage("User with this email already exists!");
            _userRepositoryMock.Verify(
                x => x.EmailExistsAsync("vyakhyanamdev@test.com"),
                Times.Once);
            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Signup_PhoneAlreadyExists_ThrowsDuplicatePhoneNumberException()
        {
            var request = GetValidRequest();
            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            Func<Task> action = () => _authService.Signup(request);
            await action.Should().ThrowAsync<DuplicatePhoneNumberException>().WithMessage("User with this phone number already exists!");
            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Signup_ValidRequest_ReturnsSignupResponse()
        {
            var request = GetValidRequest();
            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _passwordHasherMock
                .Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("HASHED_PASSWORD");
            var response = await _authService.Signup(request);
            response.Should().NotBeNull();
            response.Name.Should().Be("Vyakhya Namdev");
            response.Email.Should().Be("vyakhyanamdev@test.com");
            response.Role.Should().Be("Customer");
            response.Balance.Should().Be(1000);
            response.Message.Should().Be("User registered successfully!");
        }

        [TestMethod]
        public async Task Signup_HashesPasswordBeforeSaving()
        {
            var request = GetValidRequest();
            User savedUser = null;
            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _passwordHasherMock
                .Setup(x => x.Hash(request.Password))
                .Returns("HASHED_PASSWORD");
            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>()))
                .Callback<User>(u => savedUser = u);
            await _authService.Signup(request);
            savedUser.Should().NotBeNull();
            savedUser.Password.Should().Be("HASHED_PASSWORD");
        }

        [TestMethod]
        public async Task Signup_CallsAddUserOnce()
        {
            var request = GetValidRequest();
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("HASHED_PASSWORD");
            await _authService.Signup(request);
            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Signup_CallsSaveChangesOnce()
        {
            var request = GetValidRequest();
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("HASHED_PASSWORD");
            await _authService.Signup(request);
            _userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [TestMethod]
        public async Task Signup_CallsHashOnce()
        {
            var request = GetValidRequest();
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("HASHED_PASSWORD");
            await _authService.Signup(request);
            _passwordHasherMock.Verify(
                x => x.Hash(request.Password),
                Times.Once);
        }

        [TestMethod]
        public async Task Signup_NormalizesEmailBeforeSaving()
        {
            var request = GetValidRequest();
            User savedUser = null;
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("HASHED_PASSWORD");
            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>()))
                .Callback<User>(u => savedUser = u);
            await _authService.Signup(request);
            savedUser.Email.Should().Be("vyakhyanamdev@test.com");
        }

        [TestMethod]
        public async Task Signup_TrimsPhoneNumber()
        {
            var request = GetValidRequest();
            User savedUser = null;
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("HASHED_PASSWORD");
            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>()))
                .Callback<User>(u => savedUser = u);
            await _authService.Signup(request);
            savedUser.PhoneNumber.Should().Be("9876543210");
        }

        [TestMethod]
        public async Task Signup_AssignsCustomerRole()
        {
            var request = GetValidRequest();
            User savedUser = null;
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("HASHED_PASSWORD");
            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>()))
                .Callback<User>(u => savedUser = u);
            await _authService.Signup(request);
            savedUser.Role.Should().Be(UserRole.Customer);
        }

        [TestMethod]
        public async Task Signup_CreatesUserAddress()
        {
            var request = GetValidRequest();
            User savedUser = null;
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("HASHED_PASSWORD");
            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>()))
                .Callback<User>(u => savedUser = u);
            await _authService.Signup(request);
            savedUser.UserAddresses.Should().HaveCount(1);
        }

        [TestMethod]
        public async Task Signup_SaveChangesThrows_ExceptionPropagates()
        {
            var request = GetValidRequest();
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.PhoneNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("HASHED_PASSWORD");
            _userRepositoryMock
                .Setup(x => x.SaveChanges())
                .ThrowsAsync(new Exception("Database Error"));
            Func<Task> action = () => _authService.Signup(request);
            await action.Should().ThrowAsync<Exception>().WithMessage("Database Error");
        }

        [TestMethod]
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

            _passwordHasherMock
                .Setup(x => x.Verify(request.Password, user.Password))
                .Returns(true);

            _jwtServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            var result = await _authService.LoginAsync(request);
            result.Should().NotBeNull();
            result.AccessToken.Should().Be("access-token");
            result.RefreshToken.Should().Be("refresh-token");
            _refreshTokenRepositoryMock.Verify(
                x => x.AddAsync(It.Is<RefreshToken>(
                    token =>
                        token.UserId == user.Id &&
                        token.Token == "refresh-token")),
                Times.Once);
        }

        [TestMethod]
        public async Task LoginAsync_UserNotFound_ThrowsInvalidCredentialsException()
        {
            var request = new LoginRequest
            {
                Email = "unknown@gmail.com",
                Password = "Password@123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            Func<Task> action = () => _authService.LoginAsync(request);
            await action.Should().ThrowAsync<InvalidCredentialsException>();
        }

        [TestMethod]
        public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentialsException()
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

            _passwordHasherMock
                .Setup(x => x.Verify(request.Password, user.Password))
                .Returns(false);

            Func<Task> action = () => _authService.LoginAsync(request);
            await action.Should().ThrowAsync<InvalidCredentialsException>();
            _refreshTokenRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<RefreshToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task LoginAsync_ValidCredentials_GeneratesAccessToken()
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

            _passwordHasherMock
                .Setup(x => x.Verify(request.Password, user.Password))
                .Returns(true);

            _jwtServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            var result = await _authService.LoginAsync(request);
            _jwtServiceMock.Verify(
                x => x.GenerateAccessToken(user),
                Times.Once);
        }

        [TestMethod]
        public async Task LoginAsync_ValidCredentials_GeneratesRefreshToken()
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

            _passwordHasherMock
                .Setup(x => x.Verify(request.Password, user.Password))
                .Returns(true);

            _jwtServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            var result = await _authService.LoginAsync(request);
            _jwtServiceMock.Verify(
                x => x.GenerateRefreshToken(),
                Times.Once);
        }

        [TestMethod]
        public async Task LoginAsync_EmptyEmail_ThrowsValidEmailException()
        {
            var request = new LoginRequest
            {
                Email = "",
                Password = "vyakhya@123"
            };

            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ThrowsAsync(new ValidEmailException());

            Func<Task> action = () =>_authService.LoginAsync(request);
            await action.Should().ThrowAsync<ValidEmailException>();
        }

        [TestMethod]
        public async Task LoginAsync_EmptyPassword_ThrowsValidPasswordException()
        {
            var request = new LoginRequest
            {
                Email = "vyakhyanamdev@test.com",
                Password = ""
            };

            var user = CreateActiveUser();
            _userRepositoryMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify(request.Password, user.Password))
                .Throws(new ValidPasswordException());

            Func<Task> action = () => _authService.LoginAsync(request);
            await action.Should().ThrowAsync<ValidPasswordException>();
        }

        [TestMethod]
        public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokens()
        {
            var user = CreateActiveUser();
            var oldToken = CreateValidRefreshToken(user);

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(oldToken.Token))
                .ReturnsAsync(oldToken);

            _jwtServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("new-access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            var result = await _authService.RefreshTokenAsync(oldToken.Token);
            result.Should().NotBeNull();
            result.AccessToken.Should().Be("new-access-token");
            result.RefreshToken.Should().Be("new-refresh-token");
        }

        [TestMethod]
        public async Task RefreshTokenAsync_TokenNotFound_ThrowsInvalidCredentialsException()
        {
            _refreshTokenRepositoryMock
               .Setup(x => x.GetByTokenAsync("invalid-token"))
               .ReturnsAsync((RefreshToken)null);

            Func<Task> action = () => _authService.RefreshTokenAsync("invalid-token");
            await action.Should().ThrowAsync<InvalidRefreshTokenException>();
            _refreshTokenRepositoryMock.Verify(
                x => x.DeleteAsync(It.IsAny<RefreshToken>()),
                Times.Never);
            _jwtServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<User>()),
                Times.Never);
        }

        [TestMethod]
        public async Task RefreshTokenAsync_InactiveUser_ThrowsInvalidCredentialsException()
        {
            var user = CreateActiveUser();
            user.IsActive = false;

            var token = CreateValidRefreshToken(user);

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(token.Token))
                .ReturnsAsync(token);

            Func<Task> action = () => _authService.RefreshTokenAsync(token.Token);
            await action.Should().ThrowAsync<UserInactiveException>();
            _refreshTokenRepositoryMock.Verify(
               x => x.DeleteAsync(It.IsAny<RefreshToken>()),
               Times.Never);
            _jwtServiceMock.Verify(
                x => x.GenerateAccessToken(It.IsAny<User>()),
                Times.Never);
        }

        [TestMethod]
        public async Task RefreshTokenAsync_ValidToken_GeneratesNewRefreshToken()
        {
            var user = CreateActiveUser();
            var oldToken = CreateValidRefreshToken(user);

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(oldToken.Token))
                .ReturnsAsync(oldToken);

            _jwtServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("new-access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            await _authService.RefreshTokenAsync(oldToken.Token);
            _refreshTokenRepositoryMock.Verify(
                x => x.DeleteAsync(oldToken),
                Times.Once);
        }

        [TestMethod]
        public async Task RefreshTokenAsync_ValidToken_SavesNewRefreshToken()
        {
            var user = CreateActiveUser();
            var oldToken = CreateValidRefreshToken(user);

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(oldToken.Token))
                .ReturnsAsync(oldToken);

            _jwtServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("new-access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            await _authService.RefreshTokenAsync(oldToken.Token);
            _refreshTokenRepositoryMock.Verify(
                x => x.AddAsync(It.Is<RefreshToken>(
                    token =>
                        token.Token == "new-refresh-token" &&
                        token.UserId == user.Id)),
                Times.Once);
        }

        [TestMethod]
        public async Task RefreshTokenAsync_ValidToken_GeneratesNewAccessToken()
        {
            var user = CreateActiveUser();
            var oldToken = CreateValidRefreshToken(user);

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(oldToken.Token))
                .ReturnsAsync(oldToken);

            _jwtServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("new-access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            await _authService.RefreshTokenAsync(oldToken.Token);
            _jwtServiceMock.Verify(
                x => x.GenerateAccessToken(user),
                Times.Once);
        }

        [TestMethod]
        public async Task LogoutAsync_ValidRefreshToken_DeletesToken()
        {
            var user = CreateActiveUser();
            var refreshToken = CreateValidRefreshToken(user);

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync(refreshToken.Token))
                .ReturnsAsync(refreshToken);

            await _authService.LogoutAsync(refreshToken.Token);
            _refreshTokenRepositoryMock.Verify(
                x => x.DeleteAsync(refreshToken),
                Times.Once);
        }

        [TestMethod]
        public async Task LogoutAsync_RefreshTokenNotFound_ThrowsInvalidRefreshTokenException()
        {
            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync("unknown-token"))
                .ReturnsAsync((RefreshToken)null);

            Func<Task> action = () =>
                _authService.LogoutAsync("unknown-token");

            await action.Should().ThrowAsync<InvalidRefreshTokenException>();
            _refreshTokenRepositoryMock.Verify(
                x => x.DeleteAsync(It.IsAny<RefreshToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task LogoutAsync_EmptyRefreshToken_ThrowsInvalidRefreshTokenException()
        {
            Func<Task> action = () => _authService.LogoutAsync("");
            await action.Should().ThrowAsync<InvalidRefreshTokenException>();
            _refreshTokenRepositoryMock.Verify(
                x => x.GetByTokenAsync(It.IsAny<string>()),
                Times.Never);
            _refreshTokenRepositoryMock.Verify(
                x => x.DeleteAsync(It.IsAny<RefreshToken>()),
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
