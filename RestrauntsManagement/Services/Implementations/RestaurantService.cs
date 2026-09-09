using DotNetRestaurantManagement.Helpers;
using DotNetRestaurantManagement.Models.DTO;
using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Implementations
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IRestaurantRepository _restaurantRepository;
        public RestaurantService(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public async Task<PaginationResult<RestaurantDto>> GetPaginationResultAsync(PaginationRequest request)
        {
            var query = _restaurantRepository.GetActiveRestaurants().Select(x => new RestaurantDto
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                City = x.Address.City,
                State = x.Address.State,
                PinCode = x.Address.PinCode,
                Country = x.Address.Country,
                Cuisine = x.Cuisine.ToString()
            })
            .OrderBy(x => x.Id);

            return await PaginationHelper.CreateAsync(query, request);
        }

        public async Task<PaginationResult<MenuItemDto>> GetRestaurantMenuAsync(long restaurantId, PaginationRequest request)
        {
            var restaurant = _restaurantRepository.GetById(restaurantId);
            if (restaurant == null) throw new KeyNotFoundException("Restaurant Not Found!");
            if (!restaurant.IsActive) throw new KeyNotFoundException("Restaurant Not Available!");

            var query = _restaurantRepository.GetAvailableMenuItems(restaurantId)
                                             .Select(x => new MenuItemDto
                                             {
                                                 Id = x.Id,
                                                 Name = x.Name,
                                                 Price = x.Price,
                                                 PreparationTime = x.PreparationTime,
                                                 Category = x.Category.ToString(),
                                                 QuantityAvailable = x.QuantityAvailable
                                             })
                                             .OrderBy(x => x.Id);
            return await PaginationHelper.CreateAsync(query, request);
        }
    }
}
