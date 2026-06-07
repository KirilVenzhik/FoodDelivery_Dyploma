using Auth.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Queries;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly PlaceOrderCommandHandler _placeOrderHandler;
    private readonly ChangeOrderStatusCommandHandler _changeStatusHandler;
    private readonly GetOrderByIdQueryHandler _getByIdHandler;
    private readonly GetOrdersByClientQueryHandler _getByClientHandler;
    private readonly GetOrdersByRestaurantQueryHandler _getByRestaurantHandler;
    private readonly ICurrentUser _currentUser;

    public OrdersController(
        PlaceOrderCommandHandler placeOrderHandler,
        ChangeOrderStatusCommandHandler changeStatusHandler,
        GetOrderByIdQueryHandler getByIdHandler,
        GetOrdersByClientQueryHandler getByClientHandler,
        GetOrdersByRestaurantQueryHandler getByRestaurantHandler,
        ICurrentUser currentUser)
    {
        _placeOrderHandler = placeOrderHandler;
        _changeStatusHandler = changeStatusHandler;
        _getByIdHandler = getByIdHandler;
        _getByClientHandler = getByClientHandler;
        _getByRestaurantHandler = getByRestaurantHandler;
        _currentUser = currentUser;
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _placeOrderHandler.HandleAsync(
            new PlaceOrderCommand(_currentUser.UserId, request.RestaurantId, request.Items),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _getByIdHandler.HandleAsync(
            new GetOrderByIdQuery(id, _currentUser.UserId, _currentUser.Role ?? string.Empty),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var orders = await _getByClientHandler.HandleAsync(
            new GetOrdersByClientQuery(_currentUser.UserId),
            cancellationToken);

        return Ok(orders);
    }

    [HttpGet("restaurant/{restaurantId:guid}")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<IActionResult> GetByRestaurant(Guid restaurantId, CancellationToken ct)
        => Ok(await _getByRestaurantHandler.HandleAsync(
               new GetOrdersByRestaurantQuery(restaurantId, _currentUser.UserId), ct));

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _changeStatusHandler.HandleAsync(
            new ChangeOrderStatusCommand(id, request.Status),
            cancellationToken);

        return Ok(response);
    }
}
