using FoodDelivery.Web.Auth;
using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public class DeliveryApiClient : ApiClient
{
    public DeliveryApiClient(HttpClient http, CustomAuthStateProvider auth, IAuthService authService)
        : base(http, auth, authService) { }

    public Task<List<DeliveryResponse>?> GetPendingAsync() =>
        GetAsync<List<DeliveryResponse>>("/api/deliveries/pending");

    public Task<DeliveryResponse?> AssignAsync(Guid id) =>
        PatchAsync<DeliveryResponse>($"/api/deliveries/{id}/assign");

    public Task<List<DeliveryResponse>?> GetMyDeliveriesAsync() =>
        GetAsync<List<DeliveryResponse>>("/api/deliveries/my");

    public Task<DeliveryResponse?> PickupAsync(Guid id) =>
        PatchAsync<DeliveryResponse>($"/api/deliveries/{id}/pickup");

    public Task<DeliveryResponse?> CompleteAsync(Guid id) =>
        PatchAsync<DeliveryResponse>($"/api/deliveries/{id}/complete");

    public Task<DeliveryResponse?> GetByIdAsync(Guid id) =>
        GetAsync<DeliveryResponse>($"/api/deliveries/{id}");
}
