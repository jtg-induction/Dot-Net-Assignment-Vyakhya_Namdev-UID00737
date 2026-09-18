using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Interfaces;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Implementations
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IUserRepository _userRepository;
        public RestaurantService(IRestaurantRepository restaurantRepository, IUserRepository userRepository)
        {
            _restaurantRepository = restaurantRepository;
            _userRepository = userRepository;
        }

        public async Task<PaginationResult<RestaurantDto>> GetRestaurantsDetailsAsync(PaginationRequest request)
        {
            var query = _restaurantRepository.GetActiveRestaurants().Select(x => new RestaurantDto
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                City = x.Address.City,
                State = x.Address.State,
                PinCode = x.Address.PinCode,
                Country = x.Address.Country,
                Cuisine = x.Cuisine.ToString()
            })
            .OrderBy(x => x.Id);

            return await PaginationHelper.CreateAsync(query, request);
        }

        public async Task<PaginationResult<MenuItemDto>> GetRestaurantMenuAsync(long restaurantId, PaginationRequest request)
        {
            var restaurant = _restaurantRepository.GetById(restaurantId);
            if (restaurant == null) throw new KeyNotFoundException(ErrorMessages.RestaurantNotFound);
            if (!restaurant.IsActive) throw new KeyNotFoundException(ErrorMessages.RestaurantNotAvailable);

            var query = _restaurantRepository.GetAvailableMenuItems(restaurantId)
                                             .Select(x => new MenuItemDto
                                             {
                                                 Id = x.Id,
                                                 Name = x.Name,
                                                 Price = x.Price,
                                                 PreparationTime = x.PreparationTime,
                                                 Category = x.Category.ToString(),
                                                 QuantityAvailable = x.QuantityAvailable
                                             })
                                             .OrderBy(x => x.Id);
            return await PaginationHelper.CreateAsync(query, request);
        }
        public async Task<OnboardRestaurantResponse> OnboardRestaurant(OnboardRestaurantRequest request)
        {
            using (var transaction = _restaurantRepository.BeginTransaction())
            {
                try
                {
                    if (request == null)
                    {
                        throw new ArgumentNullException(nameof(request));
                    }

                    if (request.Owner == null)
                    {
                        throw new ApiException(HttpStatusCode.BadRequest, ErrorMessages.OwnerDetailsRequired);
                    }

                    if (request.Owner.UserId.HasValue)
                    {
                        bool hasNewUserFields =
                            !string.IsNullOrWhiteSpace(request.Owner.Name) ||
                            !string.IsNullOrWhiteSpace(request.Owner.Email) ||
                            !string.IsNullOrWhiteSpace(request.Owner.PhoneNumber) ||
                            !string.IsNullOrWhiteSpace(request.Owner.Password);
                        if (hasNewUserFields)
                        {
                            throw new ApiException(HttpStatusCode.BadRequest, ErrorMessages.ExistingUserError);
                        }
                    }

                    if (!request.Owner.UserId.HasValue)
                    {
                        if (string.IsNullOrWhiteSpace(request.Owner.Name) ||
                            string.IsNullOrWhiteSpace(request.Owner.Email) ||
                            string.IsNullOrWhiteSpace(request.Owner.PhoneNumber) ||
                            string.IsNullOrWhiteSpace(request.Owner.Password))
                        {
                            throw new ApiException(
                                HttpStatusCode.BadRequest, ErrorMessages.RequiredOwnerDetails);
                        }
                    }

                    string normalizedEmail = request.Email.Trim();
                    if (await _restaurantRepository.EmailExistsAsync(normalizedEmail))
                    {
                        throw new ApiException(HttpStatusCode.Conflict, ErrorMessages.RestaurantAlreadyExists);
                    }

                    User owner;
                    if (request.Owner.UserId.HasValue)
                    {
                        owner = await _userRepository.GetByIdAsync(request.Owner.UserId.Value);
                        if (owner == null)
                        {
                            throw new ApiException(HttpStatusCode.NotFound, ErrorMessages.OwnerNotFound);
                        }
                        owner.Role = UserRole.Owner;
                    }
                    else
                    {
                        var userNormalizedEmail = request.Owner.Email.Trim();
                        var normalizedPhoneNumber = request.Owner.PhoneNumber.Trim();
                        var existingUser = await _userRepository.EmailExistsAsync(userNormalizedEmail);
                        var existingPhoneNumber = await _userRepository.PhoneNumberExistsAsync(normalizedPhoneNumber);
                        if (existingUser)
                        {
                            throw new ApiException(HttpStatusCode.Conflict, ErrorMessages.UserAlreadyExists);
                        }
                        if(existingPhoneNumber)
                        {
                            throw new ApiException(HttpStatusCode.Conflict, ErrorMessages.PhoneNumberAlreadyExists);
                        }

                        owner = new User
                        {
                            Name = request.Owner.Name.Trim(),
                            Email = userNormalizedEmail,
                            PhoneNumber = request.Owner.PhoneNumber.Trim(),
                            Password = PasswordHashingHelper.Hash(request.Owner.Password),
                            Role = UserRole.Owner
                        };

                        _userRepository.AddUser(owner);
                        await _userRepository.SaveChangesAsync();
                    }
                    var address = new Address
                    {
                        HouseNumber = request.Address.HouseNumber.Trim(),
                        StreetAddress = request.Address.StreetAddress.Trim(),
                        City = request.Address.City.Trim(),
                        State = request.Address.State.Trim(),
                        PinCode = request.Address.PinCode.Trim(),
                        Country = request.Address.Country.Trim(),
                        AddressType = AddressType.Work
                    };

                    _restaurantRepository.AddRestaurantAddress(address);
                    await _restaurantRepository.SaveChangesAsync();

                    var restaurant = new Restaurant
                    {
                        Name = request.Name,
                        Email = request.Email,
                        OwnerId = owner.Id,
                        AddressId = address.Id
                    };

                    _restaurantRepository.Add(restaurant);
                    await _restaurantRepository.SaveChangesAsync();

                    transaction.Commit();
                    return new OnboardRestaurantResponse
                    {
                        restaurantId = restaurant.Id
                    };
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
