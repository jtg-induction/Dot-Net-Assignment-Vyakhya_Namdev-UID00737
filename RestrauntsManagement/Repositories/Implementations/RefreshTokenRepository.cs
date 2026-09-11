using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories.Interfaces;
using System.Data.Entity;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        protected readonly RestaurantDbContext _context;

        public RefreshTokenRepository(RestaurantDbContext context)
        {
            _context = context;
        }

        public Task Add(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            return Task.CompletedTask;
        }

        public async Task<RefreshToken> GetByTokenAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == tokenHash);
        }

        public async Task<RefreshToken> GetByIdAsync(long id)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task Delete(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Remove(refreshToken);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
