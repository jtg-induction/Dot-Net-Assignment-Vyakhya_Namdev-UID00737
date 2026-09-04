using DotNetRestaurantManagement.Constants;
using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository){
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }
        public async Task<SignupResponse> Signup(SignupRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            string normalizedEmail = request.Email.Trim().ToLowerInvariant();
            string normalizedPhoneNumber = request.PhoneNumber.Trim();

            if (await _userRepository.EmailExistsAsync(normalizedEmail))
            {
                throw new ApiException(HttpStatusCode.Conflict, ErrorMessages.DuplicateEmailException);
            }
            if (await _userRepository.PhoneNumberExistsAsync(normalizedPhoneNumber))
            {
                throw new ApiException(HttpStatusCode.Conflict, ErrorMessages.DuplicatePhoneNumberException);
            }

            var address = new Address
            {
                HouseNumber = request.HouseNumber.Trim(),
                StreetAddress = request.StreetAddress.Trim(),
                City = request.City.Trim(),
                State = request.State.Trim(),
                PinCode = request.PinCode.Trim(),
                Country = request.Country.Trim(),
                AddressType = request.AddressType
            };

            var user = new User
            {
                Name = request.Name.Trim(),
                Email = normalizedEmail,
                Password = PasswordHashingHelper.Hash(request.Password),
                PhoneNumber = normalizedPhoneNumber,
                Role = UserRole.Customer
            };

            var userAddress = new UserAddress
            {
                User = user,
                Address = address
            };

            user.UserAddresses.Add(userAddress);
            _userRepository.AddUser(user);
            await _userRepository.SaveChangesAsync();
            return new SignupResponse
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Balance = user.Balance
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (user == null) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            bool isValidPassword = PasswordHashingHelper.Verify(
                request.Password,
                user.Password);

            if (!isValidPassword) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            if (!user.IsActive) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.UserNotFound);
            string refreshToken = TokenHelper.GenerateRefreshToken();
            string refreshTokenHash = TokenHelper.Hash(refreshToken);
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenHash
            };

            await _refreshTokenRepository.Add(refreshTokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();
            string accessToken = TokenHelper.GenerateAccessToken(
                user,
                refreshTokenEntity.Id);
            return new LoginResponse
            {
                Name = user.Name,
                Email = user.Email,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            string refreshTokenHash = TokenHelper.Hash(refreshToken);
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenHash);
            if (existingToken == null) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            if (existingToken.CreatedAt.AddDays(ValidationConstants.RefreshTokenExpiryDays) <= DateTime.UtcNow)
            {
                throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            }

            var user = existingToken.User;
            if (user == null) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            if (!user.IsActive) throw new ApiException( HttpStatusCode.Unauthorized, ErrorMessages.UserNotFound);
            string newRefreshToken = TokenHelper.GenerateRefreshToken();
            string newRefreshTokenHash = TokenHelper.Hash(newRefreshToken);
            var newToken = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshTokenHash
            };

            await _refreshTokenRepository.Add(newToken);
            await _refreshTokenRepository.Delete(existingToken);
            await _refreshTokenRepository.SaveChangesAsync();
            string newAccessToken = TokenHelper.GenerateAccessToken(user, newToken.Id);
            return new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(long userId, long refreshTokenId)
        {
            if (userId <= 0 || refreshTokenId <= 0) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            var existingToken = await _refreshTokenRepository.GetByIdAsync(refreshTokenId);
            if (existingToken == null) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            if (existingToken.UserId != userId) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            await _refreshTokenRepository.Delete(existingToken);
            await _refreshTokenRepository.SaveChangesAsync();
        }

        public async Task<UpdateProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null){
                throw new UserNotFound();
            }

            string normalizedEmail = request.Email.Trim().ToLowerInvariant();
            string normalizedPhoneNumber = request.PhoneNumber.Trim();

            if (!string.Equals(
                    user.Email,
                    normalizedEmail,
                    StringComparison.OrdinalIgnoreCase))
            {
                bool emailExists = await _userRepository.EmailExistsForOtherUserAsync(normalizedEmail, userId);
                if (emailExists){
                    throw new DuplicateEmailException();
                }
            }

            if (!string.Equals(
                    user.PhoneNumber,
                    normalizedPhoneNumber,
                    StringComparison.OrdinalIgnoreCase))
            {
                bool phoneExists = await _userRepository.PhoneNumberExistsForOtherUserAsync(
                        normalizedPhoneNumber,
                        userId);

                if (phoneExists){
                    throw new PhoneNumberAlreadyRegistered();
                }
            }

            string normalizedName = request.Name.Trim();

            await _userRepository.UpdateProfileAsync(
                userId,
                normalizedName,
                normalizedEmail,
                normalizedPhoneNumber);

            await _userRepository.SaveChanges();

            return new UpdateProfileResponse
            {
                Id = (int)user.Id,
                Name = normalizedName,
                Email = normalizedEmail,
                PhoneNumber = normalizedPhoneNumber
            };
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request){
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new UserNotFound();
            }

            bool isCurrentPasswordValid =
                _passwordHasher.Verify(
                    request.CurrentPassword,
                    user.Password);

            if (!isCurrentPasswordValid)
            {
                throw new InvalidCredentialsException();
            }

            user.Password =
                _passwordHasher.Hash(request.NewPassword);

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.SaveChanges();
        }

        public async Task<AddressResponse> AddAddressAsync(
    int userId,
    AddressRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new UserNotFound();
            }

            var address = new Address
            {
                HouseNumber = request.HouseNumber.Trim(),
                StreetAddress = request.StreetAddress.Trim(),
                City = request.City.Trim(),
                State = request.State.Trim(),
                PinCode = request.PinCode.Trim(),
                Country = request.Country.Trim(),
                AddressType = (AddressType)request.AddressType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _userRepository.AddAddress(address);

            await _userRepository.SaveChanges();

            var userAddress = new UserAddress
            {
                UserId = userId,
                AddressId = address.Id
            };

            _userRepository.AddUserAddress(userAddress);

            await _userRepository.SaveChanges();

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
    }
}
