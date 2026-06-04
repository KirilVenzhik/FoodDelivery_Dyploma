using FoodDelivery.Web.Auth;
using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public class RestaurantApiClient : ApiClient
{
    public RestaurantApiClient(HttpClient http, CustomAuthStateProvider auth, IAuthService authService)
        : base(http, auth, authService) { }

    public Task<List<RestaurantSummaryResponse>?> GetRestaurantsAsync() =>
        GetAsync<List<RestaurantSummaryResponse>>("/api/restaurants");

    public Task<RestaurantResponse?> GetRestaurantAsync(Guid id) =>
        GetAsync<RestaurantResponse>($"/api/restaurants/{id}");
}
