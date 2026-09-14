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
        private AuthService _authService;

        [TestInitialize]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _authService = new AuthService(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object);
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

        /// <summary>
        /// Verifies that Signup throws an ArgumentNullException when the signup request is null.
        /// </summary>
        [TestMethod]
        public async Task Signup_NullRequest_ThrowsArgumentNullException()
        {
            Func<Task> action = () => _authService.Signup(null);
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        /// <summary>
        /// Verifies that Signup throws a DuplicateEmailException when the email is already registered.
        /// </summary>
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

        /// <summary>
        /// Verifies that Signup throws a DuplicatePhoneNumberException when the phone number is already registered.
        /// </summary>
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

        /// <summary>
        /// Verifies that Signup returns the expected response for a valid signup request.
        /// </summary>
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
        }

        /// <summary>
        /// Verifies that the user's password is hashed before the user is saved.
        /// </summary>
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

        /// <summary>
        /// Verifies that the email is normalized before the user is saved.
        /// </summary>
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

        /// <summary>
        /// Verifies that leading and trailing spaces are removed from the phone number before saving.
        /// </summary>
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

        /// <summary>
        /// Verifies that a newly registered user is assigned the Customer role.
        /// </summary>
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

        /// <summary>
        /// Verifies that a UserAddress is created for the newly registered user.
        /// </summary>
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

        /// <summary>
        /// Verifies that an exception thrown while saving changes is propagated to the caller.
        /// </summary>
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
    }
}
