using DotNetRestaurantManagement.Models.DTO;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Interfaces
{
    public interface IUserService
    {
        Task<UpdateProfileResponse> UpdateProfileAsync(
            long userId,
            UpdateProfileRequest request);

        Task ChangePasswordAsync(
            long userId,
            ChangePasswordRequest request);

        Task<AddressResponse> AddAddressAsync(
            long userId,
            AddressRequest request);

        Task DeactivateAccountAsync(long userId, long refreshTokenId);
    }
}
