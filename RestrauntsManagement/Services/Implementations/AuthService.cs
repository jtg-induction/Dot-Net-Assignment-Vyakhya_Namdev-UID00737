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

        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher){
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
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
    }
}