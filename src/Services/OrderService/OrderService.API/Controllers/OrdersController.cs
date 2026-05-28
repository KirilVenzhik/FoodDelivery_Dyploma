using Auth.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands;
using OrderService.Application.Common;
using OrderService.Application.DTOs;
using OrderService.Application.Queries;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly PlaceOrderCommandHandler _placeOrderHandler;
    private readonly UpdateOrderStatusCommandHandler _updateStatusHandler;
    private readonly GetOrderByIdQueryHandler _getByIdHandler;
    private readonly GetOrdersByClientQueryHandler _getByClientHandler;
    private readonly ICurrentUser _currentUser;

    public OrdersController(
        PlaceOrderCommandHandler placeOrderHandler,
        UpdateOrderStatusCommandHandler updateStatusHandler,
        GetOrderByIdQueryHandler getByIdHandler,
        GetOrdersByClientQueryHandler getByClientHandler,
        ICurrentUser currentUser)
    {
        _placeOrderHandler = placeOrderHandler;
        _updateStatusHandler = updateStatusHandler;
        _getByIdHandler = getByIdHandler;
        _getByClientHandler = getByClientHandler;
        _currentUser = currentUser;
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _placeOrderHandler.HandleAsync(
            new PlaceOrderCommand(_currentUser.UserId, request.RestaurantId, request.Items),
            cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => Ok(new { orderId = result.CreatedId }),
            CommandResultStatus.NotFound => NotFound(result.Error),
            CommandResultStatus.UnprocessableEntity => UnprocessableEntity(result.Error),
            _ => StatusCode(500)
        };
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.HandleAsync(
            new GetOrderByIdQuery(id, _currentUser.UserId, _currentUser.Role ?? string.Empty),
            cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => Ok(result.Value),
            CommandResultStatus.NotFound => NotFound(),
            CommandResultStatus.Forbidden => Forbid(),
            _ => StatusCode(500)
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var orders = await _getByClientHandler.HandleAsync(
            new GetOrdersByClientQuery(_currentUser.UserId),
            cancellationToken);

        return Ok(orders);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "RestaurantManager")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateStatusHandler.HandleAsync(
            new UpdateOrderStatusCommand(id, request.Status),
            cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => NoContent(),
            CommandResultStatus.NotFound => NotFound(),
            CommandResultStatus.UnprocessableEntity => UnprocessableEntity(result.Error),
            _ => StatusCode(500)
        };
    }
}
