using Auth.Abstractions;
using DeliveryService.Application.Commands;
using DeliveryService.Application.Common;
using DeliveryService.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.API.Controllers;

[ApiController]
[Route("api/deliveries")]
[Authorize]
public class DeliveriesController : ControllerBase
{
    private readonly AssignDeliveryCommandHandler _assignHandler;
    private readonly PickupDeliveryCommandHandler _pickupHandler;
    private readonly CompleteDeliveryCommandHandler _completeHandler;
    private readonly GetDeliveryByIdQueryHandler _getByIdHandler;
    private readonly GetDeliveriesByCourierQueryHandler _getByCourierHandler;
    private readonly ICurrentUser _currentUser;

    public DeliveriesController(
        AssignDeliveryCommandHandler assignHandler,
        PickupDeliveryCommandHandler pickupHandler,
        CompleteDeliveryCommandHandler completeHandler,
        GetDeliveryByIdQueryHandler getByIdHandler,
        GetDeliveriesByCourierQueryHandler getByCourierHandler,
        ICurrentUser currentUser)
    {
        _assignHandler = assignHandler;
        _pickupHandler = pickupHandler;
        _completeHandler = completeHandler;
        _getByIdHandler = getByIdHandler;
        _getByCourierHandler = getByCourierHandler;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> GetMyDeliveries(CancellationToken cancellationToken)
    {
        var deliveries = await _getByCourierHandler.HandleAsync(
            new GetDeliveriesByCourierQuery(_currentUser.UserId), cancellationToken);

        return Ok(deliveries);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Courier,Administrator")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.HandleAsync(
            new GetDeliveryByIdQuery(id), cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => Ok(result.Value),
            CommandResultStatus.NotFound => NotFound(),
            _ => StatusCode(500)
        };
    }

    [HttpPatch("{id:guid}/assign")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> Assign(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assignHandler.HandleAsync(
            new AssignDeliveryCommand(id, _currentUser.UserId), cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => NoContent(),
            CommandResultStatus.NotFound => NotFound(),
            CommandResultStatus.UnprocessableEntity => UnprocessableEntity(result.Error),
            _ => StatusCode(500)
        };
    }

    [HttpPatch("{id:guid}/pickup")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> Pickup(Guid id, CancellationToken cancellationToken)
    {
        var result = await _pickupHandler.HandleAsync(
            new PickupDeliveryCommand(id, _currentUser.UserId), cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => NoContent(),
            CommandResultStatus.NotFound => NotFound(),
            CommandResultStatus.Forbidden => Forbid(),
            CommandResultStatus.UnprocessableEntity => UnprocessableEntity(result.Error),
            _ => StatusCode(500)
        };
    }

    [HttpPatch("{id:guid}/deliver")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> Deliver(Guid id, CancellationToken cancellationToken)
    {
        var result = await _completeHandler.HandleAsync(
            new CompleteDeliveryCommand(id, _currentUser.UserId), cancellationToken);

        return result.Status switch
        {
            CommandResultStatus.Success => NoContent(),
            CommandResultStatus.NotFound => NotFound(),
            CommandResultStatus.Forbidden => Forbid(),
            CommandResultStatus.UnprocessableEntity => UnprocessableEntity(result.Error),
            _ => StatusCode(500)
        };
    }
}
