using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories.Interfaces;
using Effort;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Tests.Repositories
{
    [TestClass]
    public class UserRepositoryTests
    {
        private RestaurantDbContext _context;
        private UserRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var connection = DbConnectionFactory.CreateTransient();
            _context = new RestaurantDbContext(connection);
            _repository = new UserRepository(_context);
        }

        private User CreateUser()
        {
            return new User
            {
                Name = "Vyakhya",
                Email = "vyakhya@test.com",
                Password = "HASH",
                PhoneNumber = "9876543210"
            };
        }

        [TestMethod]
        [Description("Verifies that EmailExistsAsync returns true when the email already exists.")]
        public async Task EmailExistsAsync_WhenEmailExists_ReturnsTrue()
        {
            _context.Users.Add(CreateUser());
            await _context.SaveChangesAsync();
            bool result = await _repository.EmailExistsAsync("vyakhya@test.com");
            result.Should().BeTrue();
        }

        [TestMethod]
        [Description("Verifies that EmailExistsAsync returns false when the email does not exist.")]
        public async Task EmailExistsAsync_WhenEmailDoesNotExist_ReturnsFalse()
        {
            bool result = await _repository.EmailExistsAsync("abc@test.com");
            result.Should().BeFalse();
        }

        [TestMethod]
        [Description("Verifies that PhoneNumberExistsAsync returns true when the phone number already exists.")]
        public async Task PhoneNumberExistsAsync_WhenPhoneExists_ReturnsTrue()
        {
            _context.Users.Add(CreateUser());
            await _context.SaveChangesAsync();
            bool result = await _repository.PhoneNumberExistsAsync("9876543210");
            result.Should().BeTrue();
        }

        [TestMethod]
        [Description("Verifies that PhoneNumberExistsAsync returns false when the phone number does not exist.")]
        public async Task PhoneNumberExistsAsync_WhenPhoneDoesNotExist_ReturnsFalse()
        {
            bool result = await _repository.PhoneNumberExistsAsync("9999999999");
            result.Should().BeFalse();
        }

        [TestMethod]
        [Description("Verifies that AddUser inserts the user into the database.")]
        public async Task AddUser_ShouldInsertUser()
        {
            User user = CreateUser();
            _repository.AddUser(user);
            await _repository.SaveChangesAsync();
            _context.Users.Count().Should().Be(1);
            _context.Users.First().Email.Should().Be("vyakhya@test.com");
        }

        [TestMethod]
        [Description("Verifies that SaveChanges persists the added user to the database.")]
        public async Task SaveChanges_ShouldPersistUser()
        {
            User user = CreateUser();
            _repository.AddUser(user);
            await _repository.SaveChangesAsync();
            _context.Users.Any().Should().BeTrue();
        }

        [TestMethod]
        [Description("Verifies that multiple users can be added and saved successfully.")]
        public async Task AddMultipleUsers_ShouldSaveAll()
        {
            _repository.AddUser(CreateUser());
            _repository.AddUser(new User
            {
                Name = "Rahul",
                Email = "rahul@test.com",
                Password = "HASH",
                PhoneNumber = "9999999999"
            });
            await _repository.SaveChangesAsync();
            _context.Users.Count().Should().Be(2);
        }

        [TestMethod]
        [Description("Verifies that GetByEmailAsync returns the user when the email exists.")]
        public async Task GetByEmailAsync_WhenUserExists_ReturnsUser()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            User result = await _repository.GetByEmailAsync("vyakhya@test.com");
            result.Should().NotBeNull();
            result.Email.Should().Be("vyakhya@test.com");
            result.Name.Should().Be("Vyakhya");
        }

        [TestMethod]
        [Description("Verifies that GetByEmailAsync returns null when the email does not exist.")]
        public async Task GetByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            User result = await _repository.GetByEmailAsync("abc@test.com");
            result.Should().BeNull();
        }

        [TestMethod]
        [Description("Verifies that GetByEmailAsync returns the stored password hash for the matching user.")]
        public async Task GetByEmailAsync_ReturnsStoredPasswordHash()
        {
            User user = CreateUser();
            user.Password = "HASHED_PASSWORD";
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            User result = await _repository.GetByEmailAsync("vyakhya@test.com");
            result.Should().NotBeNull();
            result.Password.Should().Be("HASHED_PASSWORD");
        }

        [TestMethod]
        [Description("Verifies that GetByEmailAsync returns the complete stored user details.")]
        public async Task GetByEmailAsync_ReturnsCompleteUser()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            User result = await _repository.GetByEmailAsync("vyakhya@test.com");
            result.Should().NotBeNull();
            result.Name.Should().Be("Vyakhya");
            result.Email.Should().Be("vyakhya@test.com");
            result.PhoneNumber.Should().Be("9876543210");
            result.Password.Should().Be("HASH");
        }

        [TestMethod]
        [Description("Verifies that GetByEmailAsync returns null when a different email is provided.")]
        public async Task GetByEmailAsync_WithDifferentEmail_ReturnsNull()
        {
            _context.Users.Add(CreateUser());
            await _context.SaveChangesAsync();
            User result = await _repository.GetByEmailAsync("xyz@test.com");
            result.Should().BeNull();
        }

        [TestMethod]
        [Description("Verifies that GetByEmailAsync returns an inactive user when the matching user is inactive.")]
        public async Task GetByEmailAsync_WhenUserIsInactive_ReturnsUser()
        {
            User user = CreateUser();
            user.IsActive = false;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            User result = await _repository.GetByEmailAsync("vyakhya@test.com");
            result.Should().NotBeNull();
            result.IsActive.Should().BeFalse();
        }

        [TestMethod]
        [Description("Verifies that GetByEmailAsync returns only the user matching the requested email.")]
        public async Task GetByEmailAsync_ReturnsOnlyMatchingUser()
        {
            _context.Users.Add(new User
            {
                Name = "Vyakhya",
                Email = "vyakhya@test.com",
                Password = "HASH1",
                PhoneNumber = "9876543210"
            });

            _context.Users.Add(new User
            {
                Name = "Rahul",
                Email = "rahul@test.com",
                Password = "HASH2",
                PhoneNumber = "9999999999"
            });

            await _context.SaveChangesAsync();

            User result = await _repository.GetByEmailAsync("vyakhya@test.com");
            result.Should().NotBeNull();
            result.Name.Should().Be("Vyakhya");
            result.Email.Should().Be("vyakhya@test.com");
        }

        [TestMethod]
        [Description("Verifies that phone number exists when it belongs to another user.")]
        public async Task PhoneNumberExistsForOtherUserAsync_WhenPhoneBelongsToOtherUser_ShouldReturnTrue()
        {
            var user1 = CreateUser();
            var user2 = CreateUser();
            user2.Email = "user2@test.com";
            user2.PhoneNumber = "9999999999";

            _context.Users.Add(user1);
            _context.Users.Add(user2);
            await _context.SaveChangesAsync();

            var result = await _repository.PhoneNumberExistsForOtherUserAsync(
                "9999999999",
                (int)user1.Id);

            result.Should().BeTrue();
        }

        [TestMethod]
        [Description("Verifies that phone number does not exist for the same user.")]
        public async Task PhoneNumberExistsForOtherUserAsync_WhenPhoneBelongsToSameUser_ShouldReturnFalse()
        {
            var user = CreateUser();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.PhoneNumberExistsForOtherUserAsync(
                user.PhoneNumber,
                (int)user.Id);

            result.Should().BeFalse();
        }

        [TestMethod]
        [Description("Verifies that phone number does not exist when no user has it.")]
        public async Task PhoneNumberExistsForOtherUserAsync_WhenPhoneDoesNotExist_ShouldReturnFalse()
        {
            var user = CreateUser();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.PhoneNumberExistsForOtherUserAsync(
                "9999999999",
                (int)user.Id);

            result.Should().BeFalse();
        }

        [TestMethod]
        [Description("Verifies that active user address is returned with its address details.")]
        public async Task GetUserAddressAsync_WhenActiveAddressExists_ShouldReturnUserAddress()
        {
            var user = CreateUser();
            var address = new Address
            {
                HouseNumber = "12A",
                StreetAddress = "MG Road",
                City = "Noida",
                State = "Uttar Pradesh",
                PinCode = "110001",
                Country = "India"
            };

            _context.Users.Add(user);
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            var userAddress = new UserAddress
            {
                UserId = user.Id,
                AddressId = address.Id,
                IsActive = true
            };

            _context.UserAddresses.Add(userAddress);
            await _context.SaveChangesAsync();

            var result = await _repository.GetUserAddressAsync((int)user.Id);

            result.Should().NotBeNull();
            result.UserId.Should().Be(user.Id);
            result.AddressId.Should().Be(address.Id);
            result.IsActive.Should().BeTrue();
            result.Address.Should().NotBeNull();
            result.Address.City.Should().Be("Noida");
        }

        [TestMethod]
        [Description("Verifies that inactive user address is not returned.")]
        public async Task GetUserAddressAsync_WhenAddressIsInactive_ShouldReturnNull()
        {
            var user = CreateUser();
            var address = new Address
            {
                HouseNumber = "12A",
                StreetAddress = "MG Road",
                City = "Noida",
                State = "Uttar Pradesh",
                PinCode = "110001",
                Country = "India"
            };

            _context.Users.Add(user);
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            var userAddress = new UserAddress
            {
                UserId = user.Id,
                AddressId = address.Id,
                IsActive = false
            };

            _context.UserAddresses.Add(userAddress);
            await _context.SaveChangesAsync();

            var result = await _repository.GetUserAddressAsync((int)user.Id);

            result.Should().BeNull();
        }

        [TestMethod]
        [Description("Verifies that null is returned when user has no address.")]
        public async Task GetUserAddressAsync_WhenNoAddressExists_ShouldReturnNull()
        {
            var user = CreateUser();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.GetUserAddressAsync((int)user.Id);

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task AddUserAddress_WhenAddressIsValid_ShouldAddAddress()
        {
            var user = CreateUser();
            user.Id = 1;

            var address = new Address
            {
                HouseNumber = "12A",
                StreetAddress = "MG Road",
                City = "Noida",
                State = "Uttar Pradesh",
                PinCode = "110001",
                Country = "India"
            };

            var userAddress = new UserAddress
            {
                UserId = 1,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _repository.AddUserAddressAsync(address, userAddress);

            var result = await _context.Addresses
                .FirstOrDefaultAsync(x => x.Id == address.Id);

            result.Should().NotBeNull();
            result.HouseNumber.Should().Be("12A");
            result.City.Should().Be("Noida");

        }

        [TestMethod]
        [Description("Verifies that a user address relationship is added to the database.")]
        public async Task AddUserAddress_WhenUserAddressIsValid_ShouldAddUserAddress()
        {
            var user = CreateUser();
            user.Id = 1;

            var address = new Address
            {
                HouseNumber = "12A",
                StreetAddress = "MG Road",
                City = "Noida",
                State = "Uttar Pradesh",
                PinCode = "110001",
                Country = "India"
            };

            var userAddress = new UserAddress
            {
                UserId = 1,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _repository.AddUserAddressAsync(address, userAddress);

            var result = await _context.UserAddresses
                .FirstOrDefaultAsync(x =>
                    x.UserId == user.Id &&
                    x.AddressId == address.Id);

            result.Should().NotBeNull();
            result.UserId.Should().Be(user.Id);
            result.AddressId.Should().Be(address.Id);
            result.IsActive.Should().BeTrue();
        }

    }
}
