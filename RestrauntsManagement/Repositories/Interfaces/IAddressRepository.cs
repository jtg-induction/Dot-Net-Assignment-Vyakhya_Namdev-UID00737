using DotNetRestaurantManagement.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Interfaces
{
    public interface IAddressRepository
    {
        Task<UserAddress> GetUserAddressAsync(long userId, long addressId);
        Task<Address> GetActiveUserAddressAsync(long addressId, long userId);
        Task<List<Address>> GetAllUserAddressesAsync(long userId);
        void AddUserAddress(Address address, UserAddress userAddress);
        Task SaveChangesAsync();
    }
}
