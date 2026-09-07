using DotNetRestaurantManagement.Models.DTO;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Services.Interfaces
{
    public interface IRestaurantService
    {
        Task<PaginationResult<RestaurantDto>> GetPaginationResultAsync(PaginationRequest request);
        Task<PaginationResult<MenuItemDto>> GetRestaurantMenuAsync(long restaurantId, PaginationRequest request);
    }
}
