using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mapping;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Commands;

public record ChangeOrderStatusCommand(Guid OrderId, string NewStatus, Guid RequestingOwnerId);

public sealed class ChangeOrderStatusCommandHandler
{
    private readonly IOrderRepository _repository;
    private readonly IRestaurantServiceClient _restaurantClient;

    public ChangeOrderStatusCommandHandler(IOrderRepository repository, IRestaurantServiceClient restaurantClient)
    {
        _repository = repository;
        _restaurantClient = restaurantClient;
    }

    public async Task<OrderResponse> HandleAsync(ChangeOrderStatusCommand command, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(command.OrderId, ct)
            ?? throw new OrderNotFoundException(command.OrderId);

        var restaurant = await _restaurantClient.GetRestaurantWithItemsAsync(order.RestaurantId, ct)
            ?? throw new OrderNotFoundException($"Restaurant {order.RestaurantId} not found.");

        if (restaurant.OwnerId != command.RequestingOwnerId)
            throw new UnauthorizedAccessException("You do not own the restaurant of this order.");

        if (!Enum.TryParse<OrderStatus>(command.NewStatus, ignoreCase: true, out var newStatus))
            throw new InvalidOrderStatusTransitionException(
                $"'{command.NewStatus}' is not a valid order status.");

        order.ChangeStatus(newStatus);

        await _repository.SaveChangesAsync(ct);

        return OrderMapper.MapToResponse(order);
    }
}
