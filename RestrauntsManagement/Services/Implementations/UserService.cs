using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        public UserService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<UpdateProfileResponse> UpdateProfileAsync(
    long userId,
    UpdateProfileRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new ApiException(
                    HttpStatusCode.Unauthorized,
                    ErrorMessages.UserNotFound);
            }

            if (!user.IsActive)
            {
                throw new ApiException(
                    HttpStatusCode.Unauthorized,
                    ErrorMessages.UserNotFound);
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                user.Name = request.Name.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                string normalizedPhoneNumber = request.PhoneNumber.Trim();

                if (!string.Equals(
                        user.PhoneNumber,
                        normalizedPhoneNumber,
                        StringComparison.OrdinalIgnoreCase))
                {
                    bool phoneExists =
                        await _userRepository.PhoneNumberExistsForOtherUserAsync(
                            normalizedPhoneNumber,
                            userId);

                    if (phoneExists)
                    {
                        throw new ApiException(
                            HttpStatusCode.Conflict,
                            ErrorMessages.PhoneNumberAlreadyExists);
                    }
                }

                user.PhoneNumber = normalizedPhoneNumber;
            }

            await _userRepository.SaveChangesAsync();

            return new UpdateProfileResponse
            {
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task ChangePasswordAsync(
            long userId,
            ChangePasswordRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new ApiException(
                    HttpStatusCode.Unauthorized,
                    ErrorMessages.UserNotFound);
            }

            if (!user.IsActive)
            {
                throw new ApiException(
                    HttpStatusCode.Unauthorized,
                    ErrorMessages.UserNotFound);
            }

            bool isCurrentPasswordValid =
                PasswordHashingHelper.Verify(
                    request.CurrentPassword,
                    user.Password);

            if (!isCurrentPasswordValid)
            {
                throw new ApiException(
                    HttpStatusCode.Unauthorized,
                    ErrorMessages.InvalidCredentials);
            }

            bool isNewPasswordSameAsCurrent =
                PasswordHashingHelper.Verify(
                    request.NewPassword,
                    user.Password);

            if (isNewPasswordSameAsCurrent)
            {
                throw new ApiException(
                    HttpStatusCode.NoContent,
                    ErrorMessages.PasswordMatchesError);
            }

            user.Password =
                PasswordHashingHelper.Hash(request.NewPassword);

            await _userRepository.SaveChangesAsync();
        }

        public async Task<AddressResponse> AddAddressAsync(
            long userId,
            AddressRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new ApiException(
                    HttpStatusCode.Unauthorized,
                    ErrorMessages.UserNotFound);
            }

            if (!user.IsActive)
            {
                throw new ApiException(
                    HttpStatusCode.Unauthorized,
                    ErrorMessages.UserNotFound);
            }

            var currentUserAddress =
                await _userRepository.GetUserAddressAsync(userId);

            if (currentUserAddress != null)
                currentUserAddress.IsActive = false;

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

            await _userRepository.AddUserAddressAsync(
                address,
                userAddress);

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

        public async Task DeactivateAccountAsync(long userId, long refreshTokenId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.UserNotFound);
            }
            user.IsActive = false;

            if (refreshTokenId <= 0)
            {
                throw new ApiException(HttpStatusCode.BadRequest, 
                    ErrorMessages.InvalidRefreshToken);
            }
            var refreshToken = await _refreshTokenRepository.GetByIdAsync(refreshTokenId);

            if (refreshToken == null)
            {
                throw new ApiException(HttpStatusCode.BadRequest, 
                    ErrorMessages.InvalidRefreshToken);
            }
            if (refreshToken.UserId != userId)
            {
                throw new ApiException(HttpStatusCode.BadRequest, 
                    ErrorMessages.InvalidRefreshToken);
            }
            await _refreshTokenRepository.Delete(refreshToken);
            await _userRepository.SaveChangesAsync();
        }
    }
}
