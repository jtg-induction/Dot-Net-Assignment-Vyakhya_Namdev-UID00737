using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Implementations
{
    public class AddressRepository : IAddressRepository
    {
        protected readonly RestaurantDbContext _context;

        public AddressRepository(RestaurantDbContext context)
        {
            _context = context;
        }
        public async Task<UserAddress> GetUserAddressAsync(long userId, long addressId)
        {
            return await _context.UserAddresses
                .FirstOrDefaultAsync(ua =>
                    ua.UserId == userId &&
                    ua.AddressId == addressId &&
                    ua.IsActive);
        }

        public async Task<Address> GetActiveUserAddressAsync(long addressId, long userId)
        {
            return await _context.UserAddresses
                        .Where(x => x.UserId == userId
                                 && x.AddressId == addressId
                                 && x.IsActive)
                        .Select(x => x.Address)
                        .FirstOrDefaultAsync();
        }

        public async Task<List<Address>> GetAllUserAddressesAsync(long userId)
        {
            return await _context.UserAddresses
                .Where(ua => ua.UserId == userId && ua.IsActive)
                .Select(ua => ua.Address)
                .ToListAsync();
        }

        public void AddUserAddress(Address address, UserAddress userAddress)
        {
            _context.Addresses.Add(address);
            userAddress.Address = address;
            _context.UserAddresses.Add(userAddress);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
