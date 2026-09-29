using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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

            bool isCurrentPasswordValid =
                PasswordHashingHelper.Verify(
                    request.CurrentPassword,
                    user.Password);

            if (!isCurrentPasswordValid)
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    ErrorMessages.InvalidCredentials);
            }

            bool isNewPasswordSameAsCurrent =
                PasswordHashingHelper.Verify(
                    request.NewPassword,
                    user.Password);

            if (isNewPasswordSameAsCurrent)
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    ErrorMessages.PasswordMatchesError);
            }

            user.Password =
                PasswordHashingHelper.Hash(request.NewPassword);
            await _refreshTokenRepository.DeleteAllTokensByUserIdAsync(userId);
            await _userRepository.SaveChangesAsync();
        }

        public async Task DeactivateAccountAsync(long userId, long refreshTokenId)
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

            user.IsActive = false;

            if (refreshTokenId > 0)
            {
                var refreshToken =
                    await _refreshTokenRepository.GetByIdAsync(refreshTokenId);

                if (refreshToken != null && refreshToken.UserId == userId)
                {
                    await _refreshTokenRepository.DeleteAllTokensByUserIdAsync(userId);
                }
            }
            await _userRepository.SaveChangesAsync();
        }
    }
}
