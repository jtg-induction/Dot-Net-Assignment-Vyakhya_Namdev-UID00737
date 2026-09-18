using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories.Implementations;
using Effort;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace DotNetRestaurantManagement.Tests.Repositories
{
    [TestClass]
    public class RestaurantRepositoryTests
    {
        private RestaurantDbContext _context;
        private RestaurantRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var connection = DbConnectionFactory.CreateTransient();
            _context = new RestaurantDbContext(connection);
            _context.Users.Add(new DotNetRestaurantManagement.Models.Entities.User
            {
                Name = "Test Owner",
                Email = "owner@test.com",
                Password = "HASH",
                PhoneNumber = "9999999999"
            });
            _context.Addresses.Add(new DotNetRestaurantManagement.Models.Entities.Address
            {
                HouseNumber = "1",
                StreetAddress = "Test Street",
                City = "Delhi",
                State = "Delhi",
                PinCode = "110001",
                Country = "India"
            });

            _context.SaveChanges();
            _repository = new RestaurantRepository(_context);
        }

        private Restaurant CreateRestaurant(string name, string email, bool isActive)
        {
            return new Restaurant
            {
                Name = name,
                Email = email,
                OwnerId = _context.Users.First().Id,
                AddressId = _context.Addresses.First().Id,
                IsActive = isActive
            };
        }

        [TestMethod]
        [Description("Checks that the repository returns only active restaurants and does not return inactive ones")]
        public void GetActiveRestaurants_ShouldReturnOnlyActiveRestaurants()
        {
            _context.Restaurants.Add(
                CreateRestaurant(
                    "Spice Garden",
                    "spice@test.com",
                    true));

            _context.Restaurants.Add(
                CreateRestaurant(
                    "Dragon Wok",
                    "dragon@test.com",
                    false));

            _context.Restaurants.Add(
                CreateRestaurant(
                    "Bella Italia",
                    "bella@test.com",
                    true));

            _context.SaveChanges();
            var result = _repository
                .GetActiveRestaurants()
                .ToList();

            result.Should().HaveCount(2);
            result.Should().OnlyContain(x => x.IsActive);
            result
                .Select(x => x.Name)
                .Should()
                .BeEquivalentTo(
                    "Spice Garden",
                    "Bella Italia");
        }

        [TestMethod]
        [Description("Checks that when all restaurants are inactive, the repository returns an empty list")]
        public void GetActiveRestaurants_ShouldReturnEmpty_WhenNoRestaurantIsActive()
        {
            _context.Restaurants.Add(
                CreateRestaurant(
                    "Spice Garden",
                    "spice@test.com",
                    false));

            _context.Restaurants.Add(
                CreateRestaurant(
                    "Dragon Wok",
                    "dragon@test.com",
                    false));

            _context.SaveChanges();
            var result = _repository
                .GetActiveRestaurants()
                .ToList();
            result.Should().BeEmpty();
        }

        [TestMethod]
        [Description("Checks that when all restaurants are active, the repository returns all of them")]
        public void GetActiveRestaurants_ShouldReturnAllRestaurants_WhenAllAreActive()
        {
            _context.Restaurants.Add(
                CreateRestaurant(
                    "Spice Garden",
                    "spice@test.com",
                    true));

            _context.Restaurants.Add(
                CreateRestaurant(
                    "Dragon Wok",
                    "dragon@test.com",
                    true));

            _context.Restaurants.Add(
                CreateRestaurant(
                    "Bella Italia",
                    "bella@test.com",
                    true));

            _context.SaveChanges();
            var result = _repository
                .GetActiveRestaurants()
                .ToList();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(x => x.IsActive);
        }

        [TestMethod]
        [Description("Checks that if a restaurant has no menu items, the repository returns an empty list instead of an error")]
        public void GetAvailableMenuItems_ShouldReturnEmpty_WhenRestaurantHasNoMenuItems()
        {
            var restaurant = CreateRestaurant(
                "Spice Garden",
                "spice@test.com",
                true);

            _context.Restaurants.Add(restaurant);
            _context.SaveChanges();
            var result = _repository
                .GetAvailableMenuItems(restaurant.Id)
                .ToList();
            result.Should().BeEmpty();
        }

        [TestMethod]
        [Description("Checks when a restaurant with the given ID exists, the repository returns that restaurant with the correct ID and name")]
        public void GetById_ShouldReturnRestaurant_WhenRestaurantExists()
        {
            var restaurant = CreateRestaurant(
                "Dragon Wok",
                "dragon@test.com",
                true);

            _context.Restaurants.Add(restaurant);
            _context.SaveChanges();
            var result = _repository.GetById(restaurant.Id);
            result.Should().NotBeNull();
            result.Id.Should().Be(restaurant.Id);
            result.Name.Should().Be("Dragon Wok");
        }

        [TestMethod]
        [Description("Verify if GetById returns inactive restaurant also")]
        public void GetById_ShouldReturnInactiveRestaurant_WhenRestaurantExists()
        {
            var restaurant = CreateRestaurant(
                "Royal Spice",
                "royal@test.com",
                false);

            _context.Restaurants.Add(restaurant);
            _context.SaveChanges();
            var result = _repository.GetById(restaurant.Id);
            result.Should().NotBeNull();
            result.Id.Should().Be(restaurant.Id);
            result.IsActive.Should().BeFalse();
        }

        [TestMethod]
        [Description("Checks that EmailExistsAsync returns true when a restaurant with the given email already exists")]
        public async Task EmailExistsAsync_ShouldReturnTrue_WhenEmailExists()
        {
            var restaurant = CreateRestaurant(
                "Spice Garden",
                "spice@test.com",
                true);

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();
            var result = await _repository.EmailExistsAsync("spice@test.com");
            result.Should().BeTrue();
        }

        [TestMethod]
        [Description("Checks that EmailExistsAsync returns false when no restaurant has the given email")]
        public async Task EmailExistsAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
        {
            var result = await _repository.EmailExistsAsync("abc@test.com");
            result.Should().BeFalse();
        }

        [TestMethod]
        [Description("Checks that AddRestaurantAddress adds the address to the database context")]
        public async Task AddRestaurantAddress_ShouldAddAddress()
        {
            var address = new Address
            {
                HouseNumber = "154",
                StreetAddress = "Mall Road",
                City = "Noida",
                State = "Uttar Pradesh",
                PinCode = "201001",
                Country = "India"
            };

            _repository.AddRestaurantAddress(address);
            await _repository.SaveChangesAsync();
            var result = _context.Addresses.FirstOrDefault(x => x.HouseNumber == "154");
            result.Should().NotBeNull();
            result.StreetAddress.Should().Be("Mall Road");
            result.City.Should().Be("Noida");
        }

        [TestMethod]
        [Description("Checks that Add adds the restaurant to the database context")]
        public async Task Add_ShouldAddRestaurant()
        {
            var restaurant = CreateRestaurant(
                "Fun Food",
                "funfood@test.com",
                true);

            _repository.Add(restaurant);
            await _repository.SaveChangesAsync();
            var result = _context.Restaurants.FirstOrDefault(x => x.Email == "funfood@test.com");
            result.Should().NotBeNull();
            result.Name.Should().Be("Fun Food");
            result.OwnerId.Should().Be(restaurant.OwnerId);
            result.AddressId.Should().Be(restaurant.AddressId);
        }

        [TestMethod]
        [Description("Checks that SaveChangesAsync persists changes made through the repository")]
        public async Task SaveChangesAsync_ShouldPersistChanges()
        {
            var restaurant = CreateRestaurant(
                "Royal Spice",
                "royal@test.com",
                true);

            _repository.Add(restaurant);
            await _repository.SaveChangesAsync();
            var result = _context.Restaurants.FirstOrDefault(x => x.Email == "royal@test.com");
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [TestMethod]
        [Description("Checks that BeginTransaction returns a transaction object when a database transaction is started")]
        public void BeginTransaction_ShouldReturnTransaction()
        {
            var transaction = _repository.BeginTransaction();
            transaction.Should().NotBeNull();
            transaction.Rollback();
        }
    }
}
