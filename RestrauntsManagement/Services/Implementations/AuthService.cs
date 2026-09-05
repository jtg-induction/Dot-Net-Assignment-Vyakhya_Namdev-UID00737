using DotNetRestaurantManagement.Exceptions;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Models.Enums;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher,
                           IRefreshTokenRepository refreshTokenRepository, IJwtService jwtService){
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        public async Task<SignupResponse> Signup(SignupRequest request){
            if (request == null){
                throw new ArgumentNullException(nameof(request));
            }

            string normalizedEmail = request.Email.Trim().ToLowerInvariant();
            string normalizedPhoneNumber = request.PhoneNumber.Trim();

            if (await _userRepository.EmailExistsAsync(normalizedEmail)){
                throw new DuplicateEmailException();
            }

            if (await _userRepository.PhoneNumberExistsAsync(normalizedPhoneNumber)){
                throw new DuplicatePhoneNumberException();
            }

            var address = new Address{
                HouseNumber = request.HouseNumber.Trim(),
                StreetAddress = request.StreetAddress.Trim(),
                City = request.City.Trim(),
                State = request.State.Trim(),
                PinCode = request.PinCode.Trim(),
                Country = request.Country.Trim(),
                AddressType = request.AddressType,
            };

            var user = new User{
                Name = request.Name.Trim(),
                Email = normalizedEmail,
                Password = _passwordHasher.Hash(request.Password),
                PhoneNumber = normalizedPhoneNumber,
                Role = UserRole.Customer,
            };

            var userAddress = new UserAddress{
                User = user,
                Address = address
            };

            user.UserAddresses.Add(userAddress);
            _userRepository.AddUser(user);
            await _userRepository.SaveChanges();

            return new SignupResponse
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Balance = user.Balance,
                Message = "User registered successfully!"
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null) throw new InvalidCredentialsException();

            bool isValid = _passwordHasher.Verify(request.Password, user.Password);
            if (!isValid) throw new InvalidCredentialsException();
            // Reactivate account when user logs in again
            if (!user.IsActive){
                user.IsActive = true;
                await _userRepository.SaveChanges();
            }
            string accessToken = _jwtService.GenerateAccessToken(user);
            string refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
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
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token == null) throw new InvalidRefreshTokenException();
            var user = token.User;
            if (!user.IsActive) throw new UserInactiveException();
            string newAccessToken = _jwtService.GenerateAccessToken(user);
            string newRefreshToken = _jwtService.GenerateRefreshToken();
            await _refreshTokenRepository.DeleteAsync(token);

            var newToken = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenRepository.AddAsync(newToken);

            return new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(string refreshToken){
            if (string.IsNullOrWhiteSpace(refreshToken)) throw new InvalidRefreshTokenException();
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token == null) throw new InvalidRefreshTokenException();
            await _refreshTokenRepository.DeleteAsync(token);
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

        public async Task<AddressResponse> AddAddressAsync(int userId, AddressRequest request)
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
                CreatedAt = DateTime.UtcNow
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

        public async Task DeactivateAccountAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive) throw new UserNotFound();
            user.IsActive = false;

            // Invalidate all existing JWT access tokens
            user.TokenVersion++;
            await _userRepository.SaveChanges();
        }
    }
}
