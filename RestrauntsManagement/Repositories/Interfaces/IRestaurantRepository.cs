using DotNetRestaurantManagement.Models.Entities;
using System.Linq;

namespace DotNetRestaurantManagement.Repositories.Interfaces
{
    public interface IRestaurantRepository
    {
        IQueryable<Restaurant> GetActiveRestaurants();
        IQueryable<MenuItem> GetAvailableMenuItems(long restaurantId);
        Restaurant GetById(long restaurantId);
    }
}
