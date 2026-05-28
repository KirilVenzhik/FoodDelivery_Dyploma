using Auth.Abstractions;
using Contracts.Events;
using DeliveryService.Application.Interfaces;
using EventBus.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.API.Controllers;

[ApiController]
[Route("api/deliveries")]
[Authorize]
public class DeliveriesController : ControllerBase
{
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IEventBus _eventBus;
    private readonly ICurrentUser _currentUser;

    public DeliveriesController(
        IDeliveryRepository deliveryRepository,
        IEventBus eventBus,
        ICurrentUser currentUser)
    {
        _deliveryRepository = deliveryRepository;
        _eventBus = eventBus;
        _currentUser = currentUser;
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Courier,Administrator")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(id, cancellationToken);

        if (delivery is null)
            return NotFound();

        return Ok(new
        {
            delivery.Id,
            delivery.OrderId,
            delivery.CourierId,
            Status = delivery.Status.ToString(),
            delivery.CreatedAt,
            delivery.UpdatedAt
        });
    }

    [HttpPatch("{id:guid}/assign")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> Assign(Guid id, CancellationToken cancellationToken)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(id, cancellationToken);

        if (delivery is null)
            return NotFound();

        if (delivery.Status != Domain.Enums.DeliveryStatus.Pending)
            return UnprocessableEntity("Delivery is not in Pending status.");

        delivery.Assign(_currentUser.UserId);

        await _deliveryRepository.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/pickup")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> Pickup(Guid id, CancellationToken cancellationToken)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(id, cancellationToken);

        if (delivery is null)
            return NotFound();

        if (delivery.CourierId != _currentUser.UserId)
            return Forbid();

        if (delivery.Status != Domain.Enums.DeliveryStatus.Assigned)
            return UnprocessableEntity("Delivery is not in Assigned status.");

        delivery.Pickup();

        await _deliveryRepository.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/deliver")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> Deliver(Guid id, CancellationToken cancellationToken)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(id, cancellationToken);

        if (delivery is null)
            return NotFound();

        if (delivery.CourierId != _currentUser.UserId)
            return Forbid();

        if (delivery.Status != Domain.Enums.DeliveryStatus.InProgress)
            return UnprocessableEntity("Delivery is not in InProgress status.");

        delivery.Complete();

        await _deliveryRepository.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new DeliveryCompletedEvent
            {
                OrderId = delivery.OrderId,
                DeliveryId = delivery.Id
            },
            "delivery.completed",
            cancellationToken);

        return NoContent();
    }
}
