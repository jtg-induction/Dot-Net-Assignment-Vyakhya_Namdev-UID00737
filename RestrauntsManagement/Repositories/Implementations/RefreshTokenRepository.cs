using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories.Interfaces;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        protected readonly RestaurantDbContext _context;

        /// <summary>
        /// Initializes the refresh token repository with the database context
        /// </summary>
        public RefreshTokenRepository(RestaurantDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new refresh token to the database context
        /// </summary>
        public void Add(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
        }

        /// <summary>
        /// Gets a refresh token by its hash and includes the associated user
        /// </summary>
        public async Task<RefreshToken> GetByTokenAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == tokenHash);
        }

        /// <summary>
        /// Gets a refresh token by its ID
        /// </summary>
        public async Task<RefreshToken> GetByIdAsync(long id)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Removes the specified refresh token from the database context
        /// </summary>
        public void Delete(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Remove(refreshToken);
        }

        /// <summary>
        /// Removes all refresh tokens belonging to the specified user
        /// </summary>
        public async Task DeleteAllTokensByUserIdAsync(long userId)
        {
            var refreshTokens = await _context.RefreshTokens
                .Where(x => x.UserId == userId)
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(refreshTokens);
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
