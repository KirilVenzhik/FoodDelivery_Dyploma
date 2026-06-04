using FoodDelivery.Web.Auth;
using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public class RestaurantApiClient : ApiClient
{
    public RestaurantApiClient(HttpClient http, CustomAuthStateProvider auth, IAuthService authService)
        : base(http, auth, authService) { }

    // Read
    public Task<List<RestaurantSummaryResponse>?> GetRestaurantsAsync() =>
        GetAsync<List<RestaurantSummaryResponse>>("/api/restaurants");

    public Task<RestaurantResponse?> GetRestaurantAsync(Guid id) =>
        GetAsync<RestaurantResponse>($"/api/restaurants/{id}");

    public Task<List<RestaurantSummaryResponse>?> GetMyRestaurantsAsync() =>
        GetAsync<List<RestaurantSummaryResponse>>("/api/restaurants/my");

    // Write (RestaurantOwner)
    public Task<RestaurantResponse?> CreateRestaurantAsync(CreateRestaurantRequest req) =>
        PostAsync<RestaurantResponse>("/api/restaurants", req);

    public Task<RestaurantResponse?> UpdateRestaurantAsync(Guid id, CreateRestaurantRequest req) =>
        PutAsync<RestaurantResponse>($"/api/restaurants/{id}", req);

    public Task<object?> OpenRestaurantAsync(Guid id) =>
        PostAsync<object>($"/api/restaurants/{id}/open");

    public Task<object?> CloseRestaurantAsync(Guid id) =>
        PostAsync<object>($"/api/restaurants/{id}/close");

    public Task<MenuItemResponse?> AddMenuItemAsync(Guid restaurantId, AddMenuItemRequest req) =>
        PostAsync<MenuItemResponse>($"/api/restaurants/{restaurantId}/menu-items", req);

    public Task<MenuItemResponse?> UpdateMenuItemAsync(Guid restaurantId, Guid itemId, UpdateMenuItemRequest req) =>
        PutAsync<MenuItemResponse>($"/api/restaurants/{restaurantId}/menu-items/{itemId}", req);

    public Task<bool> DeleteMenuItemAsync(Guid restaurantId, Guid itemId) =>
        DeleteAsync($"/api/restaurants/{restaurantId}/menu-items/{itemId}");
}
