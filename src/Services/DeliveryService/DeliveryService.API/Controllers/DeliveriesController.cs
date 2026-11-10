using Auth.Abstractions;
using DeliveryService.Application.Commands;
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
    private readonly PickUpDeliveryCommandHandler _pickUpHandler;
    private readonly CompleteDeliveryCommandHandler _completeHandler;
    private readonly GetDeliveryByIdQueryHandler _getByIdHandler;
    private readonly GetMyDeliveriesQueryHandler _getMyDeliveriesHandler;
    private readonly GetPendingDeliveriesQueryHandler _getPendingHandler;
    private readonly ICurrentUser _currentUser;

    public DeliveriesController(
        AssignDeliveryCommandHandler assignHandler,
        PickUpDeliveryCommandHandler pickUpHandler,
        CompleteDeliveryCommandHandler completeHandler,
        GetDeliveryByIdQueryHandler getByIdHandler,
        GetMyDeliveriesQueryHandler getMyDeliveriesHandler,
        GetPendingDeliveriesQueryHandler getPendingHandler,
        ICurrentUser currentUser)
    {
        _assignHandler = assignHandler;
        _pickUpHandler = pickUpHandler;
        _completeHandler = completeHandler;
        _getByIdHandler = getByIdHandler;
        _getMyDeliveriesHandler = getMyDeliveriesHandler;
        _getPendingHandler = getPendingHandler;
        _currentUser = currentUser;
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var result = await _getPendingHandler.HandleAsync(new GetPendingDeliveriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
    {
        var result = await _getMyDeliveriesHandler.HandleAsync(
            new GetMyDeliveriesQuery(_currentUser.UserId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.HandleAsync(
            new GetDeliveryByIdQuery(id, _currentUser.UserId), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/assign")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> Assign(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assignHandler.HandleAsync(
            new AssignDeliveryCommand(id, _currentUser.UserId), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/pickup")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> PickUp(Guid id, CancellationToken cancellationToken)
    {
        var result = await _pickUpHandler.HandleAsync(
            new PickUpDeliveryCommand(id, _currentUser.UserId), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/complete")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _completeHandler.HandleAsync(
            new CompleteDeliveryCommand(id, _currentUser.UserId), cancellationToken);
        return Ok(result);
    }
}
