using System.Threading.Tasks;
using DotNetRestaurantManagement.Models.DTO;

namespace DotNetRestaurantManagement.Services.Interfaces
{
    public interface IAuthService
    {
        Task<SignupResponse> Signup(SignupRequest request);
    }
}
