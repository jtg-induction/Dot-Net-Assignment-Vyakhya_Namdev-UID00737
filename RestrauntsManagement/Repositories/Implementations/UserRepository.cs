using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using System;
using System.Data.Entity;
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

        public void Dispose(){
            _context.Dispose();
        }
    }
}