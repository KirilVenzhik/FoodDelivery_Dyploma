using Contracts.Events;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mapping;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Commands;

public record PlaceOrderCommand(Guid ClientId, Guid RestaurantId, List<OrderItemRequest> Items);

public sealed class PlaceOrderCommandHandler
{
    private readonly IOrderRepository _repository;
    private readonly IRestaurantServiceClient _restaurantClient;
    private readonly IOutboxService _outboxService;

    public PlaceOrderCommandHandler(
        IOrderRepository repository,
        IRestaurantServiceClient restaurantClient,
        IOutboxService outboxService)
    {
        _repository = repository;
        _restaurantClient = restaurantClient;
        _outboxService = outboxService;
    }

    public async Task<OrderResponse> HandleAsync(PlaceOrderCommand command, CancellationToken ct = default)
    {
        var restaurant = await _restaurantClient.GetRestaurantWithItemsAsync(command.RestaurantId, ct);

        if (restaurant is null)
            throw new RestaurantValidationException($"Restaurant '{command.RestaurantId}' was not found.");

        if (!restaurant.IsActive)
            throw new RestaurantValidationException("Restaurant is not currently accepting orders.");

        var orderItems = new List<OrderItem>();

        foreach (var itemRequest in command.Items)
        {
            var menuItem = restaurant.MenuItems.FirstOrDefault(m => m.Id == itemRequest.MenuItemId);

            if (menuItem is null)
                throw new RestaurantValidationException(
                    $"Menu item '{itemRequest.MenuItemId}' does not exist in this restaurant.");

            if (!menuItem.IsAvailable)
                throw new RestaurantValidationException(
                    $"Menu item '{menuItem.Name}' is not currently available.");

            orderItems.Add(OrderItem.Create(menuItem.Id, menuItem.Name, menuItem.Price, itemRequest.Quantity));
        }

        var correlationId = Guid.NewGuid();
        var order = Order.Create(command.ClientId, command.RestaurantId, orderItems, correlationId);
        order.StartPayment();

        await _repository.AddAsync(order, ct);

        await _outboxService.SaveMessageAsync(
            new PaymentRequestedEvent
            {
                CorrelationId = order.CorrelationId,
                OrderId = order.Id,
                ClientId = order.ClientId,
                Amount = order.TotalAmount
            },
            "payment.requested",
            ct);

        await _repository.SaveChangesAsync(ct);

        return OrderMapper.MapToResponse(order);
    }
}
