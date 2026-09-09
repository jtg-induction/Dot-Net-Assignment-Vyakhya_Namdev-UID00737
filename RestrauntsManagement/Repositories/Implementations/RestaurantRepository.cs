using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories.Interfaces;
using System.Linq;

namespace DotNetRestaurantManagement.Repositories.Implementations
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly RestaurantDbContext _context;
        public RestaurantRepository(RestaurantDbContext context)
        {
            _context = context;
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
            return _context.Restaurants.AsNoTracking().FirstOrDefault(r => r.Id == restaurantId);
        }
    }
}
