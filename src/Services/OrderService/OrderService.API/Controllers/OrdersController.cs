using System.Text.Json;
using Auth.Abstractions;
using Contracts.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using Shared.Primitives;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRestaurantServiceClient _restaurantClient;
    private readonly ICurrentUser _currentUser;

    public OrdersController(
        IOrderRepository orderRepository,
        IRestaurantServiceClient restaurantClient,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _restaurantClient = restaurantClient;
        _currentUser = currentUser;
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<object>> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantClient
            .GetRestaurantAsync(request.RestaurantId, cancellationToken);

        if (restaurant is null)
            return NotFound("Restaurant not found.");

        if (restaurant.Status != "Open")
            return UnprocessableEntity("Restaurant is not accepting orders.");

        var orderItems = new List<OrderItem>();

        foreach (var itemRequest in request.Items)
        {
            var menuItem = restaurant.MenuItems
                .FirstOrDefault(m => m.Id == itemRequest.MenuItemId);

            if (menuItem is null)
                return UnprocessableEntity($"Menu item {itemRequest.MenuItemId} not found.");

            if (!menuItem.IsAvailable)
                return UnprocessableEntity($"Menu item '{menuItem.Name}' is not available.");

            orderItems.Add(OrderItem.Create(
                menuItem.Id,
                menuItem.Name,
                menuItem.Price,
                itemRequest.Quantity));
        }

        var order = Order.Create(
            _currentUser.UserId,
            request.RestaurantId,
            orderItems);

        order.RequestPayment();

        var paymentRequestedEvent = new PaymentRequestedEvent
        {
            CorrelationId = order.CorrelationId,
            OrderId = order.Id,
            ClientId = order.ClientId,
            Amount = order.TotalAmount
        };

        await _orderRepository.AddAsync(order, cancellationToken);

        await _orderRepository.AddOutboxMessageAsync(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = nameof(PaymentRequestedEvent),
            Payload = JsonSerializer.Serialize(paymentRequestedEvent),
            OccurredOnUtc = DateTime.UtcNow
        }, cancellationToken);

        await _orderRepository.SaveChangesAsync(cancellationToken);

        return Ok(new { orderId = order.Id });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (order is null)
            return NotFound();

        if (order.ClientId != _currentUser.UserId
            && _currentUser.Role != "Admin")
            return Forbid();

        return Ok(MapToResponse(order));
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetMyOrders(
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository
            .GetByClientIdAsync(_currentUser.UserId, cancellationToken);

        return Ok(orders.Select(MapToResponse).ToList());
    }

    private static OrderResponse MapToResponse(Order order) =>
        new()
        {
            Id = order.Id,
            CorrelationId = order.CorrelationId,
            ClientId = order.ClientId,
            RestaurantId = order.RestaurantId,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            CreatedAtUtc = order.CreatedAtUtc,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                MenuItemId = i.MenuItemId,
                ItemName = i.ItemName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                LineTotal = i.LineTotal
            }).ToList()
        };
}
