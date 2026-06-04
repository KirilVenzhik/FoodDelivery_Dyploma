using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace FoodDelivery.Web.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private const string TokenKey = "access_token";
    private const string RefreshKey = "refresh_token";
    private readonly IJSRuntime _js;
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthStateProvider(IJSRuntime js) => _js = js;

    public async Task<string?> GetAccessTokenAsync() =>
        await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);

    public async Task<string?> GetRefreshTokenAsync() =>
        await _js.InvokeAsync<string?>("localStorage.getItem", RefreshKey);

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        var claims = ParseClaimsFromJwt(token).ToList();
        // Expiry check
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (exp is not null && long.TryParse(exp, out var seconds))
        {
            var expiry = DateTimeOffset.FromUnixTimeSeconds(seconds);
            if (expiry < DateTimeOffset.UtcNow)
                return Anonymous; // ApiClient will attempt refresh on next call
        }

        var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task MarkUserAsAuthenticatedAsync(string accessToken, string refreshToken)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, accessToken);
        await _js.InvokeVoidAsync("localStorage.setItem", RefreshKey, refreshToken);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", RefreshKey);
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    // Decode JWT payload without external libraries. Maps backend "nameid"/"role" keys.
    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2) yield break;
        var json = Convert.FromBase64String(PadBase64(parts[1]));
        var map = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        if (map is null) yield break;

        foreach (var kv in map)
        {
            var type = kv.Key switch
            {
                "nameid" or "sub" or "unique_name" => ClaimTypes.NameIdentifier,
                "role" => ClaimTypes.Role,
                "email" => ClaimTypes.Email,
                _ => kv.Key
            };

            if (kv.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in kv.Value.EnumerateArray())
                    yield return new Claim(type, item.ToString());
            }
            else
            {
                yield return new Claim(type, kv.Value.ToString());
            }
        }
    }

    private static string PadBase64(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return base64;
    }
}
