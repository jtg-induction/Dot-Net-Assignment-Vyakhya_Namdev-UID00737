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
        public AddressService(IAddressRepository addressRepository, IUserRepository userRepository)
        {
            _addressRepository = addressRepository;
            _userRepository = userRepository;
        }

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

        public async Task<AddressResponse> AddAddressAsync(
            long userId,
            AddressRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new ApiException(
                    HttpStatusCode.Unauthorized,
                    ErrorMessages.NotAuthorized);
            }

            if (!user.IsActive)
            {
                throw new ApiException(
                    HttpStatusCode.Forbidden,
                    ErrorMessages.AccessDenied);
            }

            return await CreateAddress(request, userId);
        }

        public async Task<AddressResponse> UpdateAddressAsync(long userId, long addressId, AddressRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new ApiException(
                    HttpStatusCode.Unauthorized,
                    ErrorMessages.NotAuthorized);
            }

            if (!user.IsActive)
            {
                throw new ApiException(
                    HttpStatusCode.Forbidden,
                    ErrorMessages.AccessDenied);
            }

            var currentUserAddress =
                await _addressRepository.GetUserAddressAsync(userId, addressId);

            if (currentUserAddress == null)
            {
                throw new ApiException(HttpStatusCode.NotFound, ErrorMessages.AddressNotFound);
            }

            currentUserAddress.IsActive = false;
            return await CreateAddress(request, userId);
        }

        public async Task<AddressResponse> GetAddressAsync(long userId, long addressId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ApiException(
                    HttpStatusCode.Unauthorized,
                    ErrorMessages.NotAuthorized);
            }

            if (!user.IsActive)
            {
                throw new ApiException(
                    HttpStatusCode.Forbidden,
                    ErrorMessages.AccessDenied);
            }

            var address = await _addressRepository.GetActiveUserAddressAsync(addressId, userId);
            if(address == null)
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

        public async Task<List<AddressResponse>> GetAllUserAddressesAsync(long userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new ApiException(
                    HttpStatusCode.Unauthorized,
                    ErrorMessages.NotAuthorized);
            }

            if (!user.IsActive)
            {
                throw new ApiException(
                    HttpStatusCode.Forbidden,
                    ErrorMessages.AccessDenied);
            }

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

        public async Task RemoveUserAddressAsync(long userId, long addressId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ApiException(
                    HttpStatusCode.Unauthorized,
                    ErrorMessages.NotAuthorized);
            }

            if (!user.IsActive)
            {
                throw new ApiException(
                    HttpStatusCode.Forbidden,
                    ErrorMessages.AccessDenied);
            }

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
