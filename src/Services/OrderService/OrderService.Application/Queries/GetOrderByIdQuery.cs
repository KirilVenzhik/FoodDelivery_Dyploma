using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mapping;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Queries;

public record GetOrderByIdQuery(Guid OrderId, Guid RequesterId);

public sealed class GetOrderByIdQueryHandler
{
    private readonly IOrderRepository _repository;
    private readonly IRestaurantServiceClient _restaurantClient;

    public GetOrderByIdQueryHandler(IOrderRepository repository, IRestaurantServiceClient restaurantClient)
    {
        _repository = repository;
        _restaurantClient = restaurantClient;
    }

    public async Task<OrderResponse> HandleAsync(GetOrderByIdQuery query, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(query.OrderId, ct)
            ?? throw new OrderNotFoundException(query.OrderId);

        if (order.ClientId != query.RequesterId)
        {
            var restaurant = await _restaurantClient.GetRestaurantWithItemsAsync(order.RestaurantId, ct);

            if (restaurant is null || restaurant.OwnerId != query.RequesterId)
                throw new UnauthorizedAccessException("You do not have access to this order.");
        }

        return OrderMapper.MapToResponse(order);
    }
}
