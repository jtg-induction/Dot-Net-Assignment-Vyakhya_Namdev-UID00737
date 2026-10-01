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

        /// <summary>
        /// Initializes the restaurant repository with the database context
        /// </summary>
        public RestaurantRepository(RestaurantDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Starts a database transaction
        /// </summary>
        public ITransaction BeginTransaction()
        {
            var transaction = _context.Database.BeginTransaction();
            return new Transaction(transaction);
        }

        /// <summary>
        /// Gets all active restaurants
        /// </summary>
        public IQueryable<Restaurant> GetActiveRestaurants()
        {
            return _context.Restaurants.AsNoTracking().Where(r => r.IsActive);
        }

        /// <summary>
        /// Gets the available menu items for a specific restaurant
        /// </summary>
        public IQueryable<MenuItem> GetAvailableMenuItems(long restaurantId)
        {
            return _context.MenuItems
                    .AsNoTracking()
                    .Where(m =>
                        m.RestaurantId == restaurantId &&
                        m.QuantityAvailable > 0);
        }

        /// <summary>
        /// Gets a restaurant by its ID
        /// </summary>
        public Restaurant GetById(long restaurantId)
        {
            return _context.Restaurants.Find(restaurantId);
        }

        /// <summary>
        /// Gets an active restaurant by its ID
        /// </summary>
        public Restaurant getActiveRestaurantById(long restaurantId)
        {
            return _context.Restaurants
                    .FirstOrDefault(x => x.Id == restaurantId && x.IsActive);
        }

        /// <summary>
        /// Checks whether a restaurant with the specified email already exists
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Restaurants.AnyAsync(restaurant => restaurant.Email == email);
        }

        /// <summary>
        /// Adds a restaurant address to the database context
        /// </summary>
        public void AddRestaurantAddress(Address address)
        {
            _context.Addresses.Add(address);
        }

        /// <summary>
        /// Adds a new restaurant to the database context
        /// </summary>
        public void Add(Restaurant restaurant)
        {
            _context.Restaurants.Add(restaurant);
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
