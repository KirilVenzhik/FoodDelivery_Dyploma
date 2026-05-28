using Contracts.Events;
using DeliveryService.Application.Common;
using DeliveryService.Application.Interfaces;
using DeliveryService.Domain.Enums;
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

    public async Task<CommandResult> HandleAsync(CompleteDeliveryCommand command, CancellationToken ct = default)
    {
        var delivery = await _repository.GetByIdAsync(command.DeliveryId, ct);
        if (delivery is null)
            return CommandResult.NotFound();

        if (delivery.CourierId != command.CourierId)
            return CommandResult.Forbidden();

        if (delivery.Status != DeliveryStatus.InProgress)
            return CommandResult.UnprocessableEntity("Delivery is not in InProgress status.");

        delivery.Complete();
        await _repository.SaveChangesAsync(ct);

        await _eventBus.PublishAsync(
            new DeliveryCompletedEvent
            {
                OrderId = delivery.OrderId,
                DeliveryId = delivery.Id
            },
            "delivery.completed",
            ct);

        return CommandResult.Success();
    }
}
