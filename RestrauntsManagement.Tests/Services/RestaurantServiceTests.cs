using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Implementations;
using DotNetRestaurantManagement.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Tests.Services
{
    [TestClass]
    public class RestaurantServiceTests
    {
        private Mock<IRestaurantRepository> _repositoryMock;
        private RestaurantService _service;
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<ITransaction> _transactionMock;

        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IRestaurantRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _transactionMock = new Mock<ITransaction>();
            _repositoryMock
                .Setup(x => x.BeginTransaction())
                .Returns(_transactionMock.Object);
            _service = new RestaurantService(
                _repositoryMock.Object,
                _userRepositoryMock.Object);
        }

        [TestMethod]
        [Description("Checks that restaurants are returned correctly with pagination and all restaurant details are mapped properly")]
        public async Task GetPaginationResultAsync_ShouldReturnRestaurants()
        {
            var restaurants = new List<Restaurant>
            {
                new Restaurant
                {
                    Id = 1,
                    Name = "Spice Garden",
                    IsActive = true,
                    Cuisine = CuisineType.Indian,
                    Address = new Address
                    {
                        City = "Pune",
                        State = "Maharashtra",
                        PinCode = "411001",
                        Country = "India"
                    }
                },
                new Restaurant
                {
                    Id = 2,
                    Name = "Dragon Wok",
                    IsActive = true,
                    Cuisine = CuisineType.Chinese,
                    Address = new Address
                    {
                        City = "Mumbai",
                        State = "Maharashtra",
                        PinCode = "400001",
                        Country = "India"
                    }
                }
            };

            _repositoryMock
                .Setup(x => x.GetActiveRestaurants())
                .Returns(CreateAsyncQueryable(restaurants));
            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 1
            };

            var result = await _service.GetRestaurantsDetailsAsync(request);
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(2);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(1);
            result.TotalPages.Should().Be(2);
            result.HasPreviousPage.Should().BeFalse();
            result.HasNextPage.Should().BeTrue();

            var restaurant = result.Items.First();
            restaurant.Id.Should().Be(1);
            restaurant.Name.Should().Be("Spice Garden");
            restaurant.Address.City.Should().Be("Pune");
            restaurant.Address.State.Should().Be("Maharashtra");
            restaurant.Address.PinCode.Should().Be("411001");
            restaurant.Address.Country.Should().Be("India");
            restaurant.Cuisine.Should().Be((CuisineType)1);
        }

        [TestMethod]
        [Description("Verifies when there are no restaurants, the service returns an empty result")]
        public async Task GetPaginationResultAsync_ShouldReturnEmptyResult_WhenNoRestaurantsExist()
        {
            var restaurants = new List<Restaurant>();
            _repositoryMock
                .Setup(x => x.GetActiveRestaurants())
                .Returns(CreateAsyncQueryable(restaurants));

            var request = new PaginationRequest
            {
                Page = 1,
                PageSize = 10
            };

            var result = await _service.GetRestaurantsDetailsAsync(request);
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [TestMethod]
        [Description("Verify when the restaurant does not exist, the service throws Not found")]
        public async Task GetRestaurantMenuAsync_ShouldThrow_WhenRestaurantDoesNotExist()
        {
            _repositoryMock
                .Setup(x => x.GetById(999))
                .Returns((Restaurant)null);

            var request = new PaginationRequest();
            Func<Task> action = async () => await _service.GetRestaurantMenuAsync(999,request);
            var exception = await action.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.NotFound);

            exception.Which.Message
                .Should().Be(ErrorMessages.RestaurantNotFound);
        }

        [TestMethod]
        [Description("when the restaurant exists but is inactive, the service does not return its menu")]
        public async Task GetRestaurantMenuAsync_ShouldThrow_WhenRestaurantIsInactive()
        {
            var restaurant = new Restaurant
            {
                Id = 10,
                Name = "Masala Junction",
                IsActive = false
            };

            _repositoryMock
                .Setup(x => x.GetById(10))
                .Returns(restaurant);

            var request = new PaginationRequest();
            Func<Task> action = async () => await _service.GetRestaurantMenuAsync(10,request);
            var exception = await action.Should()
                .ThrowAsync<ApiException>();

            exception.Which.StatusCode
                .Should().Be(HttpStatusCode.NotFound);

            exception.Which.Message
                .Should().Be(ErrorMessages.RestaurantNotFound);
        }

        private static IQueryable<T> CreateAsyncQueryable<T>(IEnumerable<T> data)
        {
            return new TestAsyncEnumerable<T>(data);
        }

        [TestMethod]
        [Description("Verify OnboardRestaurant throws BadRequest when owner details are not provided")]
        public async Task OnboardRestaurant_ShouldThrowBadRequest_WhenOwnerIsNull()
        {
            var request = new OnboardRestaurantRequest
            {
                Name = "Saini Restaurant",
                Email = "saini@test.com",
                Owner = null
            };

            Func<Task> action = async () => await _service.OnboardRestaurant(request);

            await action.Should()
                .ThrowAsync<ApiException>()
                .WithMessage(ErrorMessages.OwnerDetailsRequired);
        }

        [TestMethod]
        [Description("Verify OnboardRestaurant ignores additional owner fields when an existing user is selected")]
        public async Task OnboardRestaurant_ShouldCreateRestaurant_WhenExistingOwnerContainsAdditionalFields()
        {
            var request = new OnboardRestaurantRequest
            {
                Name = "Saini Restaurant",
                Email = "saini@test.com",
                Address = new AddressRequest
                {
                    HouseNumber = "154",
                    StreetAddress = "Mall Road",
                    City = "Noida",
                    State = "Uttar Pradesh",
                    PinCode = "201001",
                    Country = "India"
                },
                Owner = new OwnerRequest
                {
                    UserId = 10,
                    Name = "Rahul Sharma",
                    Email = "rahul@test.com",
                    PhoneNumber = "9876543210",
                    Password = "rahul@123"
                }
            };

            var owner = new User
            {
                Id = 10,
                Name = "Existing User",
                Email = "existing@test.com",
                PhoneNumber = "9876543210",
                Role = UserRole.Customer
            };

            _repositoryMock
                .Setup(x => x.EmailExistsAsync("saini@test.com"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(owner);

            _repositoryMock
                .Setup(x => x.AddRestaurantAddress(It.IsAny<Address>()));

            _repositoryMock
                .Setup(x => x.Add(It.IsAny<Restaurant>()))
                .Callback<Restaurant>(restaurant => restaurant.Id = 1);

            var result = await _service.OnboardRestaurant(request);

            result.Should().NotBeNull();
            result.restaurantId.Should().Be(1);
            owner.Role.Should().Be(UserRole.Owner);

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(10),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.Add(
                    It.Is<Restaurant>(r =>
                        r.Name == "Saini Restaurant" &&
                        r.Email == "saini@test.com" &&
                        r.Owner == owner)),
                Times.Once);

            _repositoryMock.Verify(
                x => x.AddRestaurantAddress(It.IsAny<Address>()),
                Times.Once);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [TestMethod]
        [Description("Verify OnboardRestaurant throws Conflict when a restaurant with the same email already exists")]
        public async Task OnboardRestaurant_ShouldThrowConflict_WhenRestaurantEmailAlreadyExists()
        {
            var request = new OnboardRestaurantRequest
            {
                Name = "Saini Restaurant",
                Email = "saini@test.com",
                Owner = new OwnerRequest
                {
                    UserId = 1
                }
            };

            _repositoryMock
                .Setup(x => x.EmailExistsAsync("saini@test.com"))
                .ReturnsAsync(true);

            Func<Task> action = async () => await _service.OnboardRestaurant(request);

            await action.Should()
                .ThrowAsync<ApiException>()
                .WithMessage(ErrorMessages.RestaurantAlreadyExists);

            _userRepositoryMock.Verify(
                x => x.GetByIdAsync(It.IsAny<int>()),
                Times.Never);

            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.Add(It.IsAny<Restaurant>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verify OnboardRestaurant throws NotFound when the selected existing owner does not exist")]
        public async Task OnboardRestaurant_ShouldThrowNotFound_WhenExistingOwnerDoesNotExist()
        {
            var request = new OnboardRestaurantRequest
            {
                Name = "Saini Restaurant",
                Email = "saini@test.com",
                Owner = new OwnerRequest
                {
                    UserId = 10
                }
            };

            _repositoryMock
                .Setup(x => x.EmailExistsAsync("saini@test.com"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync((User)null);

            Func<Task> action = async () => await _service.OnboardRestaurant(request);

            await action.Should()
                .ThrowAsync<ApiException>()
                .WithMessage(ErrorMessages.UserNotFound);

            _repositoryMock.Verify(
                x => x.Add(It.IsAny<Restaurant>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verify OnboardRestaurant throws Conflict when the new owner's email already exists")]
        public async Task OnboardRestaurant_ShouldThrowConflict_WhenOwnerEmailAlreadyExists()
        {
            var request = new OnboardRestaurantRequest
            {
                Name = "Saini Restaurant",
                Email = "saini@test.com",
                Owner = new OwnerRequest
                {
                    Name = "Rahul Sharma",
                    Email = "rahul@test.com",
                    PhoneNumber = "9876543210",
                    Password = "rahul@123"
                }
            };

            _repositoryMock
                .Setup(x => x.EmailExistsAsync("saini@test.com"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync("rahul@test.com"))
                .ReturnsAsync(true);

            Func<Task> action = async () => await _service.OnboardRestaurant(request);

            await action.Should()
                .ThrowAsync<ApiException>()
                .WithMessage(ErrorMessages.UserAlreadyExists);

            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.Add(It.IsAny<Restaurant>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verify OnboardRestaurant throws Conflict when the new owner's phone number already exists")]
        public async Task OnboardRestaurant_ShouldThrowConflict_WhenOwnerPhoneNumberAlreadyExists()
        {
            var request = new OnboardRestaurantRequest
            {
                Name = "Saini Restaurant",
                Email = "saini@test.com",
                Owner = new OwnerRequest
                {
                    Name = "Rahul Sharma",
                    Email = "rahul@test.com",
                    PhoneNumber = "9876543210",
                    Password = "rahul@123"
                }
            };

            _repositoryMock
                .Setup(x => x.EmailExistsAsync("saini@test.com"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync("rahul@test.com"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsAsync("9876543210"))
                .ReturnsAsync(true);

            Func<Task> action = async () => await _service.OnboardRestaurant(request);

            await action.Should()
                .ThrowAsync<ApiException>()
                .WithMessage(ErrorMessages.PhoneNumberAlreadyExists);

            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.Add(It.IsAny<Restaurant>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [TestMethod]
        [Description("Verify OnboardRestaurant successfully creates a restaurant using an existing user as owner")]
        public async Task OnboardRestaurant_ShouldCreateRestaurant_WhenExistingOwnerIsValid()
        {
            var request = new OnboardRestaurantRequest
            {
                Name = "Saini Restaurant",
                Email = "saini@test.com",
                Address = new AddressRequest
                {
                    HouseNumber = "154",
                    StreetAddress = "Mall Road",
                    City = "Noida",
                    State = "Uttar Pradesh",
                    PinCode = "201001",
                    Country = "India"
                },
                Owner = new OwnerRequest
                {
                    UserId = 10
                }
            };

            var owner = new User
            {
                Id = 10,
                Name = "Rahul Sharma",
                Email = "rahul@test.com",
                PhoneNumber = "9876543210",
                Role = UserRole.Customer
            };

            _repositoryMock
                .Setup(x => x.EmailExistsAsync("saini@test.com"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(10))
                .ReturnsAsync(owner);

            _repositoryMock
                .Setup(x => x.AddRestaurantAddress(It.IsAny<Address>()));

            _repositoryMock
                .Setup(x => x.Add(It.IsAny<Restaurant>()))
                .Callback<Restaurant>(restaurant => restaurant.Id = 1);

            var result = await _service.OnboardRestaurant(request);

            result.Should().NotBeNull();
            result.restaurantId.Should().Be(1);
            owner.Role.Should().Be(UserRole.Owner);

            _repositoryMock.Verify(
                x => x.AddRestaurantAddress(
                    It.Is<Address>(a =>
                        a.HouseNumber == "154" &&
                        a.StreetAddress == "Mall Road" &&
                        a.City == "Noida" &&
                        a.State == "Uttar Pradesh" &&
                        a.PinCode == "201001" &&
                        a.Country == "India" &&
                        a.AddressType == AddressType.Work)),
                Times.Once);

            _repositoryMock.Verify(
                x => x.Add(
                    It.Is<Restaurant>(r =>
                        r.Name == "Saini Restaurant" &&
                        r.Email == "saini@test.com" &&
                        r.Owner == owner)),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.AddUser(It.IsAny<User>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [TestMethod]
        [Description("Verify OnboardRestaurant successfully creates a new owner and restaurant")]
        public async Task OnboardRestaurant_ShouldCreateRestaurant_WhenNewOwnerIsValid()
        {
            var request = new OnboardRestaurantRequest
            {
                Name = "Saini Restaurant",
                Email = "saini@test.com",
                Address = new AddressRequest
                {
                    HouseNumber = "154",
                    StreetAddress = "Mall Road",
                    City = "Noida",
                    State = "Uttar Pradesh",
                    PinCode = "201001",
                    Country = "India"
                },
                Owner = new OwnerRequest
                {
                    Name = "Rahul Sharma",
                    Email = "rahul@test.com",
                    PhoneNumber = "9876543210",
                    Password = "rahul@123"
                }
            };

            _repositoryMock
                .Setup(x => x.EmailExistsAsync("saini@test.com"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync("rahul@test.com"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsAsync("9876543210"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.AddUser(It.IsAny<User>()));

            _repositoryMock
                .Setup(x => x.AddRestaurantAddress(It.IsAny<Address>()));

            _repositoryMock
                .Setup(x => x.Add(It.IsAny<Restaurant>()))
                .Callback<Restaurant>(restaurant => restaurant.Id = 1);

            var result = await _service.OnboardRestaurant(request);

            result.Should().NotBeNull();
            result.restaurantId.Should().Be(1);

            _userRepositoryMock.Verify(
                x => x.AddUser(
                    It.Is<User>(u =>
                        u.Name == "Rahul Sharma" &&
                        u.Email == "rahul@test.com" &&
                        u.PhoneNumber == "9876543210" &&
                        u.Role == UserRole.Owner &&
                        !string.IsNullOrEmpty(u.Password))),
                Times.Once);

            _repositoryMock.Verify(
                x => x.AddRestaurantAddress(
                    It.Is<Address>(a =>
                        a.HouseNumber == "154" &&
                        a.StreetAddress == "Mall Road" &&
                        a.City == "Noida" &&
                        a.State == "Uttar Pradesh" &&
                        a.PinCode == "201001" &&
                        a.Country == "India" &&
                        a.AddressType == AddressType.Work)),
                Times.Once);

            _repositoryMock.Verify(
                x => x.Add(
                    It.Is<Restaurant>(r =>
                        r.Name == "Saini Restaurant" &&
                        r.Email == "saini@test.com" &&
                        r.Owner != null &&
                        r.Owner.Name == "Rahul Sharma" &&
                        r.Owner.Email == "rahul@test.com" &&
                        r.Owner.Role == UserRole.Owner)),
                Times.Once);

            _repositoryMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [TestMethod]
        [Description("Verify OnboardRestaurant trims restaurant, owner and address fields before saving")]
        public async Task OnboardRestaurant_ShouldTrimInputValues_WhenValidRequestIsProvided()
        {
            var request = new OnboardRestaurantRequest
            {
                Name = "  Saini Restaurant  ",
                Email = "  saini@test.com  ",
                Address = new AddressRequest
                {
                    HouseNumber = " 154 ",
                    StreetAddress = " Mall Road ",
                    City = " Noida ",
                    State = " Uttar Pradesh ",
                    PinCode = " 201001 ",
                    Country = " India "
                },
                Owner = new OwnerRequest
                {
                    Name = " Rahul Sharma ",
                    Email = " rahul@test.com ",
                    PhoneNumber = " 9876543210 ",
                    Password = "rahul@123"
                }
            };

            _repositoryMock
                .Setup(x => x.EmailExistsAsync("saini@test.com"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync("rahul@test.com"))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.PhoneNumberExistsAsync("9876543210"))
                .ReturnsAsync(false);

            _repositoryMock
                .Setup(x => x.AddRestaurantAddress(It.IsAny<Address>()));

            _repositoryMock
                .Setup(x => x.Add(It.IsAny<Restaurant>()))
                .Callback<Restaurant>(restaurant => restaurant.Id = 1);

            var result = await _service.OnboardRestaurant(request);

            result.Should().NotBeNull();

            _userRepositoryMock.Verify(
                x => x.AddUser(
                    It.Is<User>(u =>
                        u.Name == "Rahul Sharma" &&
                        u.Email == "rahul@test.com" &&
                        u.PhoneNumber == "9876543210" &&
                        u.Role == UserRole.Owner)),
                Times.Once);

            _repositoryMock.Verify(
                x => x.AddRestaurantAddress(
                    It.Is<Address>(a =>
                        a.HouseNumber == "154" &&
                        a.StreetAddress == "Mall Road" &&
                        a.City == "Noida" &&
                        a.State == "Uttar Pradesh" &&
                        a.PinCode == "201001" &&
                        a.Country == "India")),
                Times.Once);

            _repositoryMock.Verify(
                x => x.Add(
                    It.Is<Restaurant>(r =>
                        r.Name == "Saini Restaurant" &&
                        r.Email == "saini@test.com")),
                Times.Once);
        }
    }
}
