using DeliveryService.Application.Common;
using DeliveryService.Application.Interfaces;
using DeliveryService.Domain.Enums;

namespace DeliveryService.Application.Commands;

public record PickupDeliveryCommand(Guid DeliveryId, Guid CourierId);

public sealed class PickupDeliveryCommandHandler
{
    private readonly IDeliveryRepository _repository;

    public PickupDeliveryCommandHandler(IDeliveryRepository repository)
        => _repository = repository;

    public async Task<CommandResult> HandleAsync(PickupDeliveryCommand command, CancellationToken ct = default)
    {
        var delivery = await _repository.GetByIdAsync(command.DeliveryId, ct);
        if (delivery is null)
            return CommandResult.NotFound();

        if (delivery.CourierId != command.CourierId)
            return CommandResult.Forbidden();

        if (delivery.Status != DeliveryStatus.Assigned)
            return CommandResult.UnprocessableEntity("Delivery is not in Assigned status.");

        delivery.Pickup();
        await _repository.SaveChangesAsync(ct);

        return CommandResult.Success();
    }
}
