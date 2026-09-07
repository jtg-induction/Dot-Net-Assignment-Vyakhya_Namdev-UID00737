using DotNetRestaurantManagement.Models.Entities;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Interfaces
{ 
    public interface IRefreshTokenRepository
    {
        void Add(RefreshToken refreshToken);
        Task<RefreshToken> GetByIdAsync(long id);
        Task<RefreshToken> GetByTokenAsync(string token);
        void Delete(RefreshToken refreshToken);
        Task SaveChangesAsync();
    }
}
