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
    }
}
