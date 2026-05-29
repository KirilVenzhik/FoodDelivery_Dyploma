using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface IRestaurantServiceClient
{
    Task<RestaurantDto?> GetRestaurantWithItemsAsync(
        Guid restaurantId,
        CancellationToken cancellationToken = default);
}
