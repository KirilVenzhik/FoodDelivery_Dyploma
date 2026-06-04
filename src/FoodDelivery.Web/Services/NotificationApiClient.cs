using FoodDelivery.Web.Auth;
using FoodDelivery.Web.Models;

namespace FoodDelivery.Web.Services;

public class NotificationApiClient : ApiClient
{
    public NotificationApiClient(HttpClient http, CustomAuthStateProvider auth, IAuthService authService)
        : base(http, auth, authService) { }

    public Task<List<NotificationResponse>?> GetNotificationsAsync() =>
        GetAsync<List<NotificationResponse>>("/api/notifications");

    public Task<NotificationResponse?> MarkReadAsync(Guid id) =>
        PatchAsync<NotificationResponse>($"/api/notifications/{id}/read");
}
