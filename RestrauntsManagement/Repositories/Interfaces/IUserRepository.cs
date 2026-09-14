using DotNetRestaurantManagement.Models.Entities;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task<bool> PhoneNumberExistsAsync(string phoneNumber);

        void AddUser(User user);

        Task SaveChangesAsync();
    }
}
