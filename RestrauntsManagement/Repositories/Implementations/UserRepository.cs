using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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

        public async Task<User> GetByIdAsync(long userId)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);
        }
    }
}
