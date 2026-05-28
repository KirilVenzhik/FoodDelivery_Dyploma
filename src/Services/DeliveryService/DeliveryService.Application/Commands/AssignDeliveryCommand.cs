using DeliveryService.Application.Common;
using DeliveryService.Application.Interfaces;
using DeliveryService.Domain.Enums;

namespace DeliveryService.Application.Commands;

public record AssignDeliveryCommand(Guid DeliveryId, Guid CourierId);

public sealed class AssignDeliveryCommandHandler
{
    private readonly IDeliveryRepository _repository;

    public AssignDeliveryCommandHandler(IDeliveryRepository repository)
        => _repository = repository;

    public async Task<CommandResult> HandleAsync(AssignDeliveryCommand command, CancellationToken ct = default)
    {
        var delivery = await _repository.GetByIdAsync(command.DeliveryId, ct);
        if (delivery is null)
            return CommandResult.NotFound();

        if (delivery.Status != DeliveryStatus.Pending)
            return CommandResult.UnprocessableEntity("Delivery is not in Pending status.");

        delivery.Assign(command.CourierId);
        await _repository.SaveChangesAsync(ct);

        return CommandResult.Success();
    }
}
