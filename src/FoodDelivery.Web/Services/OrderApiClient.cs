using FoodDelivery.Web.Auth;
using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public class OrderApiClient : ApiClient
{
    public OrderApiClient(HttpClient http, CustomAuthStateProvider auth, IAuthService authService)
        : base(http, auth, authService) { }

    public Task<List<OrderResponse>?> GetOrdersAsync() =>
        GetAsync<List<OrderResponse>>("/api/orders");

    public Task<OrderResponse?> GetOrderAsync(Guid id) =>
        GetAsync<OrderResponse>($"/api/orders/{id}");

    public Task<OrderResponse?> CreateOrderAsync(CreateOrderRequest request) =>
        PostAsync<OrderResponse>("/api/orders", request);
}
