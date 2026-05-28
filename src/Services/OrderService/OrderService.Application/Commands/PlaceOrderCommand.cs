using System.Text.Json;
using Contracts.Events;
using OrderService.Application.Common;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using Shared.Primitives;

namespace OrderService.Application.Commands;

public record PlaceOrderCommand(Guid ClientId, Guid RestaurantId, List<OrderItemRequest> Items);

public sealed class PlaceOrderCommandHandler
{
    private readonly IOrderRepository _repository;
    private readonly IRestaurantServiceClient _restaurantClient;

    public PlaceOrderCommandHandler(
        IOrderRepository repository,
        IRestaurantServiceClient restaurantClient)
    {
        _repository = repository;
        _restaurantClient = restaurantClient;
    }

    public async Task<CommandResult> HandleAsync(PlaceOrderCommand command, CancellationToken ct = default)
    {
        var restaurant = await _restaurantClient.GetRestaurantAsync(command.RestaurantId, ct);

        if (restaurant is null)
            return CommandResult.NotFound("Restaurant not found.");

        if (restaurant.Status != "Open")
            return CommandResult.UnprocessableEntity("Restaurant is not accepting orders.");

        var orderItems = new List<OrderItem>();

        foreach (var itemRequest in command.Items)
        {
            var menuItem = restaurant.MenuItems.FirstOrDefault(m => m.Id == itemRequest.MenuItemId);

            if (menuItem is null)
                return CommandResult.UnprocessableEntity($"Menu item {itemRequest.MenuItemId} not found.");

            if (!menuItem.IsAvailable)
                return CommandResult.UnprocessableEntity($"Menu item '{menuItem.Name}' is not available.");

            orderItems.Add(OrderItem.Create(menuItem.Id, menuItem.Name, menuItem.Price, itemRequest.Quantity));
        }

        var order = Order.Create(command.ClientId, command.RestaurantId, orderItems);
        order.RequestPayment();

        var paymentEvent = new PaymentRequestedEvent
        {
            CorrelationId = order.CorrelationId,
            OrderId = order.Id,
            ClientId = order.ClientId,
            Amount = order.TotalAmount
        };

        await _repository.AddAsync(order, ct);
        await _repository.AddOutboxMessageAsync(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = nameof(PaymentRequestedEvent),
            Payload = JsonSerializer.Serialize(paymentEvent),
            OccurredOnUtc = DateTime.UtcNow
        }, ct);
        await _repository.SaveChangesAsync(ct);

        return CommandResult.Success(order.Id);
    }
}
