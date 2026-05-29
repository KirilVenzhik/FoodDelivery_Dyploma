using Contracts.Events;
using DeliveryService.Application.DTOs;
using DeliveryService.Application.Interfaces;
using DeliveryService.Domain.Exceptions;
using EventBus.Abstractions;

namespace DeliveryService.Application.Commands;

public record CompleteDeliveryCommand(Guid DeliveryId, Guid CourierId);

public sealed class CompleteDeliveryCommandHandler
{
    private readonly IDeliveryRepository _repository;
    private readonly IEventBus _eventBus;

    public CompleteDeliveryCommandHandler(IDeliveryRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task<DeliveryResponse> HandleAsync(CompleteDeliveryCommand command, CancellationToken ct = default)
    {
        var delivery = await _repository.GetByIdAsync(command.DeliveryId, ct)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        if (delivery.CourierId != command.CourierId)
            throw new UnauthorizedAccessException("Only the assigned courier can complete this delivery.");

        delivery.Complete();

        await _eventBus.PublishAsync(
            new DeliveryCompletedEvent
            {
                OrderId = delivery.OrderId,
                DeliveryId = delivery.Id,
                CorrelationId = delivery.CorrelationId,
                CourierId = delivery.CourierId!.Value
            },
            "delivery.completed",
            ct);

        await _repository.SaveChangesAsync(ct);
        return DeliveryResponse.FromDomain(delivery);
    }
}
