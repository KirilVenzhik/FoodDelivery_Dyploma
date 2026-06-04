using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FoodDelivery.Web.Auth;

namespace FoodDelivery.Web.Services;

// Base class for RestaurantApiClient / OrderApiClient / DeliveryApiClient /
// NotificationApiClient / PaymentApiClient (added by the role sessions).
// Attaches the Bearer token and retries once after a 401 via token refresh.
public abstract class ApiClient
{
    protected readonly HttpClient Http;
    private readonly CustomAuthStateProvider _auth;
    private readonly IAuthService _authService;

    protected ApiClient(HttpClient http, CustomAuthStateProvider auth, IAuthService authService)
    {
        Http = http;
        _auth = auth;
        _authService = authService;
    }

    private async Task AttachTokenAsync()
    {
        var token = await _auth.GetAccessTokenAsync();
        Http.DefaultRequestHeaders.Authorization =
            string.IsNullOrWhiteSpace(token) ? null : new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> call)
    {
        await AttachTokenAsync();
        var resp = await call();
        if (resp.StatusCode == HttpStatusCode.Unauthorized && await _authService.TryRefreshAsync())
        {
            await AttachTokenAsync();
            resp = await call();
        }
        return resp;
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        var resp = await SendAsync(() => Http.GetAsync(url));
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<T>() : default;
    }

    protected async Task<T?> PostAsync<T>(string url, object? body = null)
    {
        var resp = await SendAsync(() => Http.PostAsJsonAsync(url, body ?? new { }));
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<T>() : default;
    }

    protected async Task<T?> PutAsync<T>(string url, object body)
    {
        var resp = await SendAsync(() => Http.PutAsJsonAsync(url, body));
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<T>() : default;
    }

    protected async Task<T?> PatchAsync<T>(string url, object? body = null)
    {
        var resp = await SendAsync(() =>
        {
            var req = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = JsonContent.Create(body ?? new { })
            };
            return Http.SendAsync(req);
        });
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<T>() : default;
    }

    protected async Task<bool> DeleteAsync(string url)
    {
        var resp = await SendAsync(() => Http.DeleteAsync(url));
        return resp.IsSuccessStatusCode;
    }
}
