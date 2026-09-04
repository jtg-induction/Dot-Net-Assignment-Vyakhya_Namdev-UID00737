using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Interfaces
{
    public class UserRepository : IUserRepository, IDisposable
    {
        private readonly RestaurantDbContext _context;
        public UserRepository(RestaurantDbContext context){
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User> GetByEmailAsync(string email){
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email){
            return await _context.Users.AnyAsync(user => user.Email == email);
        }

        public async Task<bool> PhoneNumberExistsAsync(string phoneNumber){
            return await _context.Users.AnyAsync(user => user.PhoneNumber == phoneNumber);
        }

        public void AddUser(User user){
            _context.Users.Add(user);
        }

        public async Task SaveChanges(){
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetByIdAsync(int userId){
            return await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);
        }
        public async Task<bool> EmailExistsForOtherUserAsync(string email, int userId){
            return await _context.Users
                .AnyAsync(user =>
                    user.Email == email &&
                    user.Id != userId);
        }
        public async Task<bool> PhoneNumberExistsForOtherUserAsync(string phoneNumber, int userId){
            return await _context.Users
                .AnyAsync(user =>
                    user.PhoneNumber == phoneNumber &&
                    user.Id != userId);
        }

        public async Task<Address> GetUserAddressAsync(int userId){
            return await _context.UserAddresses
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.Address)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateProfileAsync(int userId,
            string name,
            string email,
            string phoneNumber)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return;
            user.Name = name;
            user.Email = email;
            user.PhoneNumber = phoneNumber;
            user.UpdatedAt = DateTime.UtcNow;
        }
        public void AddAddress(Address address){
            _context.Addresses.Add(address);
        }

        public void AddUserAddress(UserAddress userAddress){
            _context.UserAddresses.Add(userAddress);
        }
        public void Dispose(){
            _context.Dispose();
        }
    }
}
