using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Implementations
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserValidatorService _userValidatorService;

        /// <summary>
        /// Initializes the address service with the required repositories
        /// </summary>
        public AddressService(IAddressRepository addressRepository, 
                              IUserRepository userRepository,
                              IUserValidatorService userValidatorService)
        {
            _addressRepository = addressRepository;
            _userRepository = userRepository;
            _userValidatorService = userValidatorService;
        }

        /// <summary>
        /// Creates a new address and associates it with the specified user
        /// </summary>
        private async Task<AddressResponse> CreateAddress(
            AddressRequest request,
            long userId)
        {
            var address = new Address
            {
                HouseNumber = request.HouseNumber.Trim(),
                StreetAddress = request.StreetAddress.Trim(),
                City = request.City.Trim(),
                State = request.State.Trim(),
                PinCode = request.PinCode.Trim(),
                Country = request.Country.Trim(),
                AddressType = (AddressType)request.AddressType
            };

            var userAddress = new UserAddress
            {
                UserId = userId,
                Address = address
            };

            _addressRepository.AddUserAddress(address, userAddress);
            await _addressRepository.SaveChangesAsync();
            return new AddressResponse
            {
                Id = address.Id,
                HouseNumber = address.HouseNumber,
                StreetAddress = address.StreetAddress,
                City = address.City,
                State = address.State,
                PinCode = address.PinCode,
                Country = address.Country,
                AddressType = (int)address.AddressType
            };
        }

        /// <summary>
        /// Adds a new address for the specified user
        /// </summary>
        public async Task<AddressResponse> AddAddressAsync(
            long userId,
            AddressRequest request)
        {
            await _userValidatorService.GetActiveUserAsync(userId);
            return await CreateAddress(request, userId);
        }

        /// <summary>
        /// Deactivates the existing address and creates a new address for the specified user
        /// </summary>
        public async Task<AddressResponse> UpdateAddressAsync(long userId, long addressId, AddressRequest request)
        {
            await _userValidatorService.GetActiveUserAsync(userId);

            var currentUserAddress =
                await _addressRepository.GetUserAddressAsync(userId, addressId);

            if (currentUserAddress == null)
            {
                throw new ApiException(HttpStatusCode.NotFound, ErrorMessages.AddressNotFound);
            }

            currentUserAddress.IsActive = false;
            return await CreateAddress(request, userId);
        }

        /// <summary>
        /// Gets an active address belonging to the specified user
        /// </summary>
        public async Task<AddressResponse> GetAddressAsync(long userId, long addressId)
        {
            await _userValidatorService.GetActiveUserAsync(userId);
            var address = await _addressRepository.GetActiveUserAddressAsync(addressId, userId);
            if (address == null)
            {
                throw new ApiException(HttpStatusCode.NotFound,
                                       ErrorMessages.AddressNotFound);
            }
            return new AddressResponse
            {
                Id = address.Id,
                HouseNumber = address.HouseNumber,
                StreetAddress = address.StreetAddress,
                City = address.City,
                State = address.State,
                PinCode = address.PinCode,
                Country = address.Country,
                AddressType = (int)address.AddressType
            };
        }

        /// <summary>
        /// Gets all active addresses belonging to the specified user
        /// </summary>
        public async Task<List<AddressResponse>> GetAllUserAddressesAsync(long userId)
        {
            await _userValidatorService.GetActiveUserAsync(userId);
            var addresses = await _addressRepository.GetAllUserAddressesAsync(userId);

            return addresses.Select(address => new AddressResponse
            {
                Id = address.Id,
                HouseNumber = address.HouseNumber,
                StreetAddress = address.StreetAddress,
                City = address.City,
                State = address.State,
                PinCode = address.PinCode,
                Country = address.Country,
                AddressType = (int)address.AddressType
            }).ToList();
        }

        /// <summary>
        /// Deactivates an address belonging to the specified user
        /// </summary>
        public async Task RemoveUserAddressAsync(long userId, long addressId)
        {
            await _userValidatorService.GetActiveUserAsync(userId);
            var address = await _addressRepository.GetUserAddressAsync(userId, addressId);
            if (address == null)
            {
                throw new ApiException(HttpStatusCode.NotFound, ErrorMessages.AddressNotFound);
            }
            address.IsActive = false;
            await _addressRepository.SaveChangesAsync();
        }
    }
}
