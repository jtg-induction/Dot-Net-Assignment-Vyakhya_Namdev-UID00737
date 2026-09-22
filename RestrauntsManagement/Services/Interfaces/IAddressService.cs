using DotNetRestaurantManagement.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Interfaces
{
    public interface IAddressService
    {
        Task<AddressResponse> AddAddressAsync(
            long userId,
            AddressRequest request);

        Task<AddressResponse> UpdateAddressAsync(
            long userId,
            long addressId,
            AddressRequest request
         );

        Task<List<AddressResponse>> GetAllUserAddressesAsync(long userId);

        Task<AddressResponse> GetAddressAsync(long userId, long addressId);
        Task RemoveUserAddressAsync(long userId, long addressId);
    }
}
