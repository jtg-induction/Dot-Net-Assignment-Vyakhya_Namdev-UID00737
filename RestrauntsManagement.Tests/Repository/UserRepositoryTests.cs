using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories.Interfaces;
using Effort;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
        public async Task EmailExistsAsync_WhenEmailExists_ReturnsTrue()
        {
            _context.Users.Add(CreateUser());
            await _context.SaveChangesAsync();
            bool result = await _repository.EmailExistsAsync("vyakhya@test.com");
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task EmailExistsAsync_WhenEmailDoesNotExist_ReturnsFalse()
        {
            bool result = await _repository.EmailExistsAsync("abc@test.com");
            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task PhoneNumberExistsAsync_WhenPhoneExists_ReturnsTrue()
        {
            _context.Users.Add(CreateUser());
            await _context.SaveChangesAsync();
            bool result = await _repository.PhoneNumberExistsAsync("9876543210");
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task PhoneNumberExistsAsync_WhenPhoneDoesNotExist_ReturnsFalse()
        {
            bool result = await _repository.PhoneNumberExistsAsync("9999999999");
            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task AddUser_ShouldInsertUser()
        {
            User user = CreateUser();
            _repository.AddUser(user);
            await _repository.SaveChanges();
            _context.Users.Count().Should().Be(1);
            _context.Users.First().Email.Should().Be("vyakhya@test.com");
        }

        [TestMethod]
        public async Task SaveChanges_ShouldPersistUser()
        {
            User user = CreateUser();
            _repository.AddUser(user);
            await _repository.SaveChanges();
            _context.Users.Any().Should().BeTrue();
        }

        [TestMethod]
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
            await _repository.SaveChanges();
            _context.Users.Count().Should().Be(2);
        }

        [TestMethod]
        public async Task SaveChanges_WithoutUsers_ShouldNotThrowException()
        {
            Func<Task> action = async () => await _repository.SaveChanges();
            await action.Should().NotThrowAsync();
        }


        [TestMethod]
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
        public async Task GetByEmailAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            User result = await _repository.GetByEmailAsync("abc@test.com");
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetByEmailAsync_WithMultipleUsers_ReturnsCorrectUser()
        {
            _context.Users.Add(CreateUser());
            _context.Users.Add(new User
            {
                Name = "Rahul",
                Email = "rahul@test.com",
                Password = "HASH2",
                PhoneNumber = "9999999999"
            });

            await _context.SaveChangesAsync();
            User result = await _repository.GetByEmailAsync("rahul@test.com");
            result.Should().NotBeNull();
            result.Name.Should().Be("Rahul");
            result.Email.Should().Be("rahul@test.com");
        }

        [TestMethod]
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
        public async Task GetByEmailAsync_WhenEmailIsNull_ReturnsNull()
        {
            User result = await _repository.GetByEmailAsync(null);
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetByEmailAsync_WhenEmailIsEmpty_ReturnsNull()
        {
            _context.Users.Add(CreateUser());
            await _context.SaveChangesAsync();
            User result = await _repository.GetByEmailAsync("");
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetByEmailAsync_WithDifferentEmail_ReturnsNull()
        {
            _context.Users.Add(CreateUser());
            await _context.SaveChangesAsync();
            User result = await _repository.GetByEmailAsync("xyz@test.com");
            result.Should().BeNull();
        }

        [TestMethod]
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
    }
}
