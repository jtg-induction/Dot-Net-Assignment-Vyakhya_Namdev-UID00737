using DotNetRestaurantManagement.Models.Entities;

using System.Threading.Tasks;namespace DotNetRestaurantManagement.Services.Interfaces
{
    public interface IUserValidatorService
    {
        Task<User> GetActiveUserAsync(long userId);

    }
}
