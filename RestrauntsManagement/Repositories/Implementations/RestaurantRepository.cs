using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories.Interfaces;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Implementations
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly RestaurantDbContext _context;
        public RestaurantRepository(RestaurantDbContext context)
        {
            _context = context;
        }

        public ITransaction BeginTransaction()
        {
            var transaction = _context.Database.BeginTransaction();
            return new Transaction(transaction);
        }

        public IQueryable<Restaurant> GetActiveRestaurants()
        {
            return _context.Restaurants.AsNoTracking().Where(r => r.IsActive);
        }

        public IQueryable<MenuItem> GetAvailableMenuItems(long restaurantId)
        {
            return _context.MenuItems
                    .AsNoTracking()
                    .Where(m =>
                        m.RestaurantId == restaurantId &&
                        m.QuantityAvailable > 0);
        }

        public Restaurant GetById(long restaurantId)
        {
            return _context.Restaurants.First(r => r.Id == restaurantId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Restaurants.AnyAsync(restaurant => restaurant.Email == email);
        }

        public void AddRestaurantAddress(Address address)
        {
            _context.Addresses.Add(address);
        }

        public void Add(Restaurant restaurant)
        {
            _context.Restaurants.Add(restaurant);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
