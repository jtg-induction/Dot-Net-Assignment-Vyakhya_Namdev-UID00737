using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Interfaces
{
    public class UserRepository : IUserRepository
    {
        protected readonly RestaurantDbContext _context;

        public UserRepository(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByEmailAsync(string email){
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email){
            return await _context.Users.AnyAsync(user => user.Email == email);
        }

        public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
        {
            return await _context.Users.AnyAsync(user => user.PhoneNumber == phoneNumber);
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<bool> PhoneNumberExistsForOtherUserAsync(string phoneNumber, long userId){
            return await _context.Users
                .AnyAsync(user =>
                    user.PhoneNumber == phoneNumber &&
                    user.Id != userId);
        }

        public async Task<UserAddress> GetUserAddressAsync(long userId){
            return await _context.UserAddresses
                .Include(ua => ua.Address)
                .FirstOrDefaultAsync(ua =>
                    ua.UserId == userId &&
                    ua.IsActive);
        }
        public async Task AddUserAddressAsync(Address address, UserAddress userAddress)
        {
            _context.Addresses.Add(address);
            userAddress.Address = address;
            _context.UserAddresses.Add(userAddress);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetByIdAsync(long userId)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);
        }
    }
}
