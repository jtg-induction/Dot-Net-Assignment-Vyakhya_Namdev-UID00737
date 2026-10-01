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

        /// <summary>
        /// Initializes the address repository with the database context
        /// </summary>
        public AddressRepository(RestaurantDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets an active address mapping for a specific user and address
        /// </summary>
        public async Task<UserAddress> GetUserAddressAsync(long userId, long addressId)
        {
            return await _context.UserAddresses
                .FirstOrDefaultAsync(ua =>
                    ua.UserId == userId &&
                    ua.AddressId == addressId &&
                    ua.IsActive);
        }

        /// <summary>
        /// Gets an active address belonging to a specific user
        /// </summary>
        public async Task<Address> GetActiveUserAddressAsync(long addressId, long userId)
        {
            return await _context.UserAddresses
                        .Where(x => x.UserId == userId
                                 && x.AddressId == addressId
                                 && x.IsActive)
                        .Select(x => x.Address)
                        .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets all active addresses belonging to a specific user
        /// </summary>
        public async Task<List<Address>> GetAllUserAddressesAsync(long userId)
        {
            return await _context.UserAddresses
                .Where(ua => ua.UserId == userId && ua.IsActive)
                .Select(ua => ua.Address)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new address and links it to the specified user
        /// </summary>
        public void AddUserAddress(Address address, UserAddress userAddress)
        {
            _context.Addresses.Add(address);
            userAddress.Address = address;
            _context.UserAddresses.Add(userAddress);
        }

        /// <summary>
        /// Checks whether an active address belongs to the specified user
        /// </summary>
        public async Task<bool> DoesAddressBelongsToUserAsync(
            long userId,
            long addressId)
        {
            return await _context.UserAddresses
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.AddressId == addressId &&
                    x.IsActive);
        }

        /// <summary>
        /// Saves the pending changes to the database
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
