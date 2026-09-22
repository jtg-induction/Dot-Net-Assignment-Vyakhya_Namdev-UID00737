using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories.Implementations;
using FluentAssertions;
using Effort;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Data.Entity;

namespace RestrauntsManagement.Tests.Repository
{
    public class AddressRepositoryTests
    {
        private RestaurantDbContext _context;
        private AddressRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var connection = DbConnectionFactory.CreateTransient();
            _context = new RestaurantDbContext(connection);
            _repository = new AddressRepository(_context);
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

            var result = await _repository.GetUserAddressAsync(user.Id, address.Id);

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

            var result = await _repository.GetUserAddressAsync(user.Id, address.Id);

            result.Should().BeNull();
        }

        [TestMethod]
        [Description("Verifies that null is returned when user has no address.")]
        public async Task GetUserAddressAsync_WhenNoAddressExists_ShouldReturnNull()
        {
            var user = CreateUser();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.GetUserAddressAsync(user.Id, 999999L);

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
            _repository.AddUserAddress(address, userAddress);
            await _context.SaveChangesAsync();


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
            _repository.AddUserAddress(address, userAddress);
            await _context.SaveChangesAsync();

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
