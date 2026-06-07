using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mapping;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Queries;

public record GetOrdersByRestaurantQuery(Guid RestaurantId, Guid RequestingOwnerId);

public sealed class GetOrdersByRestaurantQueryHandler
{
    private readonly IOrderRepository _repository;
    private readonly IRestaurantServiceClient _restaurantClient;

    public GetOrdersByRestaurantQueryHandler(IOrderRepository repository, IRestaurantServiceClient restaurantClient)
    {
        _repository = repository;
        _restaurantClient = restaurantClient;
    }

    public async Task<List<OrderResponse>> HandleAsync(GetOrdersByRestaurantQuery query, CancellationToken ct = default)
    {
        var restaurant = await _restaurantClient.GetRestaurantWithItemsAsync(query.RestaurantId, ct);

        if (restaurant is null)
            throw new OrderNotFoundException($"Restaurant {query.RestaurantId} not found.");

        if (restaurant.OwnerId != query.RequestingOwnerId)
            throw new UnauthorizedAccessException("You do not own this restaurant.");

        var orders = await _repository.GetByRestaurantIdAsync(query.RestaurantId, ct);
        return orders.Select(OrderMapper.MapToResponse).ToList();
    }
}
