using System.Net.Http.Json;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Infrastructure.Messaging;

public class RestaurantServiceHttpClient : IRestaurantServiceClient
{
    private readonly HttpClient _httpClient;

    public RestaurantServiceHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RestaurantDto?> GetRestaurantAsync(
        Guid restaurantId,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<RestaurantDto>(
            $"api/restaurants/{restaurantId}",
            cancellationToken);
    }
}
