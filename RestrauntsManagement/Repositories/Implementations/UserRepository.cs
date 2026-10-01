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

        /// <summary>
        /// Initializes the user repository with the database context
        /// </summary>
        public UserRepository(RestaurantDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a user by their email address
        /// </summary>
        public async Task<User> GetByEmailAsync(string email){
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        /// <summary>
        /// Checks whether a user with the specified email address already exists
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email){
            return await _context.Users.AnyAsync(user => user.Email == email);
        }

        /// <summary>
        /// Checks whether a user with the specified phone number already exists
        /// </summary>
        public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
        {
            return await _context.Users.AnyAsync(user => user.PhoneNumber == phoneNumber);
        }

        /// <summary>
        /// Adds a new user to the database context
        /// </summary>
        public void AddUser(User user)
        {
            _context.Users.Add(user);
        }

        /// <summary>
        /// Saves the pending changes to the database
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks whether the specified phone number is already used by another user
        /// </summary>
        public async Task<bool> PhoneNumberExistsForOtherUserAsync(string phoneNumber, long userId){
            return await _context.Users
                .AnyAsync(user =>
                    user.PhoneNumber == phoneNumber &&
                    user.Id != userId);
        }

        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        public async Task<User> GetByIdAsync(long userId)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);
        }

        /// <summary>
        /// Gets an active user by their ID
        /// </summary>
        public async Task<User> GetActiveUserByIdAsync(
            long userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Id == userId &&
                    x.IsActive);
        }
    }
}
