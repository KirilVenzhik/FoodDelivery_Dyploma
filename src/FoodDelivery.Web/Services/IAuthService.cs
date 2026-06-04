using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(string email, string password);
    Task<AuthResponse?> RegisterAsync(string email, string password, string role);
    Task<bool> TryRefreshAsync();
    Task<UserDto?> GetCurrentUserAsync();
    Task LogoutAsync();
}
