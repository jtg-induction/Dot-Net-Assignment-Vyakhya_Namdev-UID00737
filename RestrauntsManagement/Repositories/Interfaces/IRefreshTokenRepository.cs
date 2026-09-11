using DotNetRestaurantManagement.Models.Entities;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Interfaces
{ 
    public interface IRefreshTokenRepository
    {
        Task Add(RefreshToken refreshToken);
        Task<RefreshToken> GetByIdAsync(long id);
        Task<RefreshToken> GetByTokenAsync(string token);
        Task Delete(RefreshToken refreshToken);
        Task SaveChangesAsync();
    }
}
