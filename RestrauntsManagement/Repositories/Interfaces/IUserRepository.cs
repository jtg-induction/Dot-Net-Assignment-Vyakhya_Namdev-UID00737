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
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(long userId);
        Task<UserAddress> GetUserAddressAsync(long userId);
        Task<bool> PhoneNumberExistsForOtherUserAsync(string phoneNumber, long userId);
        Task AddUserAddressAsync(Address address, UserAddress userAddress);
    }
}
