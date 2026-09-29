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

            var user = new User
            {
                Name = request.Name.Trim(),
                Email = normalizedEmail,
                Password = PasswordHashingHelper.Hash(request.Password),
                PhoneNumber = normalizedPhoneNumber,
                Role = UserRole.Customer
            };

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
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (user == null) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            bool isValidPassword = PasswordHashingHelper.Verify(
                request.Password,
                user.Password);

            if (!isValidPassword) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            if (!user.IsActive) throw new ApiException(HttpStatusCode.Forbidden, ErrorMessages.AccountDeactivated);
            string refreshToken = TokenHelper.GenerateRefreshToken();
            string refreshTokenHash = TokenHelper.Hash(refreshToken);
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenHash
            };

            _refreshTokenRepository.Add(refreshTokenEntity);
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
            string refreshTokenHash = TokenHelper.Hash(refreshToken);
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenHash);
            if (existingToken == null) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            if (existingToken.CreatedAt.AddDays(ValidationConstants.RefreshTokenExpiryDays) <= DateTime.UtcNow)
            {
                throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            }

            var user = existingToken.User;
            if (user == null) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.InvalidCredentials);
            if (!user.IsActive) throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.NotAuthorized);
            string newRefreshToken = TokenHelper.GenerateRefreshToken();
            string newRefreshTokenHash = TokenHelper.Hash(newRefreshToken);
            var newToken = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshTokenHash
            };

            _refreshTokenRepository.Add(newToken);
            _refreshTokenRepository.Delete(existingToken);
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
            var existingToken = await _refreshTokenRepository.GetByIdAsync(refreshTokenId);
            if (existingToken == null) return;
            if (existingToken.UserId != userId)
            {
                throw new ApiException(HttpStatusCode.Unauthorized, ErrorMessages.NotAuthorized);
            }
            _refreshTokenRepository.Delete(existingToken);
            await _refreshTokenRepository.SaveChangesAsync();
        }
    }
}
