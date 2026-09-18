using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Models.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Interfaces
{
    public interface IRestaurantRepository
    {
        ITransaction BeginTransaction();
        IQueryable<Restaurant> GetActiveRestaurants();
        IQueryable<MenuItem> GetAvailableMenuItems(long restaurantId);
        Restaurant GetById(long restaurantId);
        Restaurant getActiveRestaurantById(long restaurantId);
        Task<bool> EmailExistsAsync(string email);
        void AddRestaurantAddress(Address address);
        Task SaveChangesAsync();
        void Add(Restaurant restaurant);
    }
}
