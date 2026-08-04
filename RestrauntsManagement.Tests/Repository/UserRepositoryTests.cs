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
    }
}