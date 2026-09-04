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
        Task<User> GetByIdAsync(int userId);
        Task<bool> EmailExistsForOtherUserAsync(string email, int userId);
        Task<bool> PhoneNumberExistsForOtherUserAsync(string phoneNumber, int userId);
        Task UpdateProfileAsync(
            int userId,
            string name,
            string email,
            string phoneNumber);
        void AddAddress(Address address);
        void AddUserAddress(UserAddress userAddress);
    }
}
