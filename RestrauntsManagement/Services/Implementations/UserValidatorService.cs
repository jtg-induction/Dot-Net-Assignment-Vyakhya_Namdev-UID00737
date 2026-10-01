using DotNetRestaurantManagement.Repositories.Interfaces;
using DotNetRestaurantManagement.Services.Interfaces;
using DotNetRestaurantManagement.Models.Entities;
using System.Threading.Tasks;
using System.Web;
using DotNetRestaurantManagement.Exceptions;
using System.Net;
using DotNetRestaurantManagement.Constants;

namespace DotNetRestaurantManagement.Services.Implementations
{
    public class UserValidatorService : IUserValidatorService
    {
        private readonly IUserRepository _userRepository;
        public UserValidatorService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetActiveUserAsync(long userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if(user == null)
            {
                throw new ApiException(HttpStatusCode.Unauthorized,
                                       ErrorMessages.NotAuthorized);
            }

            if (!user.IsActive)
            {
                throw new ApiException(HttpStatusCode.Forbidden,
                                        ErrorMessages.AccessDenied);
            }

            return user;
        }
    }
}
