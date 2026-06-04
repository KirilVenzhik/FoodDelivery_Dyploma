using System.Net.Http.Json;
using FoodDelivery.Web.Auth;
using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _auth;

    public AuthService(HttpClient http, CustomAuthStateProvider auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        var resp = await _http.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        if (!resp.IsSuccessStatusCode) return null;
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is not null)
            await _auth.MarkUserAsAuthenticatedAsync(auth.AccessToken, auth.RefreshToken);
        return auth;
    }

    public async Task<AuthResponse?> RegisterAsync(string email, string password, string role)
    {
        var resp = await _http.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, password, role));
        if (!resp.IsSuccessStatusCode) return null;
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is not null)
            await _auth.MarkUserAsAuthenticatedAsync(auth.AccessToken, auth.RefreshToken);
        return auth;
    }

    public async Task<bool> TryRefreshAsync()
    {
        var refresh = await _auth.GetRefreshTokenAsync();
        if (string.IsNullOrWhiteSpace(refresh)) return false;

        var resp = await _http.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(refresh));
        if (!resp.IsSuccessStatusCode)
        {
            await _auth.MarkUserAsLoggedOutAsync();
            return false;
        }
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is null) return false;
        await _auth.MarkUserAsAuthenticatedAsync(auth.AccessToken, auth.RefreshToken);
        return true;
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        var token = await _auth.GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        try { return await _http.GetFromJsonAsync<UserDto>("/api/auth/me"); }
        catch { return null; }
    }

    public Task LogoutAsync() => _auth.MarkUserAsLoggedOutAsync();
}
