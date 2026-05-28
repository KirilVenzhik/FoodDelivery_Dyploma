using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface IRestaurantServiceClient
{
    Task<RestaurantDto?> GetRestaurantAsync(
        Guid restaurantId,
        CancellationToken cancellationToken = default);
}
