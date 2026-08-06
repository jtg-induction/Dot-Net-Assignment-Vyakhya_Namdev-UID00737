using DotNetRestaurantManagement.Models.Entities;

namespace DotNetRestaurantManagement.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
