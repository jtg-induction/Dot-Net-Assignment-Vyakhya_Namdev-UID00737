using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using System.Collections.Generic;
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
        Task DeactivateAccountAsync(long userId, long refreshTokenId);
    }
}
