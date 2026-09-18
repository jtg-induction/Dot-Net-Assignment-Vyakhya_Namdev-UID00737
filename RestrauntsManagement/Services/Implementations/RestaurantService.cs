using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Interfaces;
using System.Net;
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
                Address = new RestaurantAddressDto
                {
                    HouseNumber = x.Address.HouseNumber,
                    StreetAddress = x.Address.StreetAddress,
                    City = x.Address.City,
                    State = x.Address.State,
                    PinCode = x.Address.PinCode,
                    Country = x.Address.Country
                },
                Cuisine = x.Cuisine
            })
            .OrderBy(x => x.Id);

            return await PaginationHelper.CreateAsync(query, request);
        }

        public async Task<PaginationResult<MenuItemDto>> GetRestaurantMenuAsync(long restaurantId, PaginationRequest request)
        {
            var restaurant = _restaurantRepository.getActiveRestaurantById(restaurantId);
            if (restaurant == null) throw new ApiException(HttpStatusCode.NotFound, ErrorMessages.RestaurantNotFound);

            var query = _restaurantRepository.GetAvailableMenuItems(restaurantId)
                                             .Select(x => new MenuItemDto
                                             {
                                                 Id = x.Id,
                                                 Name = x.Name,
                                                 Price = x.Price,
                                                 PreparationTime = x.PreparationTime,
                                                 Category = x.Category,
                                                 QuantityAvailable = x.QuantityAvailable
                                             })
                                             .OrderBy(x => x.Id);
            return await PaginationHelper.CreateAsync(query, request);
        }
        private async Task<User> OnboardRestaurantOwner(OwnerRequest ownerRequest)
        {
            User owner;
            if (ownerRequest.UserId.HasValue)
            {
                owner = await _userRepository.GetByIdAsync(
                    ownerRequest.UserId.Value);

                if (owner == null)
                {
                    throw new ApiException(
                        HttpStatusCode.NotFound,
                        ErrorMessages.UserNotFound);
                }

                owner.Role = UserRole.Owner;
                return owner;
            }

            string userNormalizedEmail = ownerRequest.Email.Trim();
            string normalizedPhoneNumber = ownerRequest.PhoneNumber.Trim();

            bool existingUser =
                await _userRepository.EmailExistsAsync(userNormalizedEmail);

            if (existingUser)
            {
                throw new ApiException(
                    HttpStatusCode.Conflict,
                    ErrorMessages.UserAlreadyExists);
            }

            bool existingPhoneNumber =
                await _userRepository.PhoneNumberExistsAsync(
                    normalizedPhoneNumber);

            if (existingPhoneNumber)
            {
                throw new ApiException(
                    HttpStatusCode.Conflict,
                    ErrorMessages.PhoneNumberAlreadyExists);
            }

            owner = new User
            {
                Name = ownerRequest.Name.Trim(),
                Email = userNormalizedEmail,
                PhoneNumber = normalizedPhoneNumber,
                Password = PasswordHashingHelper.Hash(ownerRequest.Password),
                Role = UserRole.Owner
            };

            _userRepository.AddUser(owner);
            return owner;
        }

        public async Task<OnboardRestaurantResponse> OnboardRestaurant(
            OnboardRestaurantRequest request)
        {
            if (request.Owner == null)
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    ErrorMessages.OwnerDetailsRequired);
            }

            string normalizedEmail = request.Email.Trim();

            if (await _restaurantRepository.EmailExistsAsync(normalizedEmail))
            {
                throw new ApiException(
                    HttpStatusCode.Conflict,
                    ErrorMessages.RestaurantAlreadyExists);
            }

            User owner = await OnboardRestaurantOwner(request.Owner);
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

            var restaurant = new Restaurant
            {
                Name = request.Name.Trim(),
                Email = normalizedEmail,
                Cuisine = request.Cuisine,
                Owner = owner,
                Address = address
            };

            _restaurantRepository.Add(restaurant);
            await _restaurantRepository.SaveChangesAsync();
            return new OnboardRestaurantResponse
            {
                restaurantId = restaurant.Id
            };
        }
    }
}
