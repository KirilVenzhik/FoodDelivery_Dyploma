using DeliveryService.Application.DTOs;
using DeliveryService.Application.Interfaces;
using DeliveryService.Domain.Exceptions;

namespace DeliveryService.Application.Commands;

public record PickUpDeliveryCommand(Guid DeliveryId, Guid CourierId);

public sealed class PickUpDeliveryCommandHandler
{
    private readonly IDeliveryRepository _repository;

    public PickUpDeliveryCommandHandler(IDeliveryRepository repository)
        => _repository = repository;

    public async Task<DeliveryResponse> HandleAsync(PickUpDeliveryCommand command, CancellationToken ct = default)
    {
        var delivery = await _repository.GetByIdAsync(command.DeliveryId, ct)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        if (delivery.CourierId != command.CourierId)
            throw new UnauthorizedAccessException("Only the assigned courier can pick up this delivery.");

        delivery.PickUp();
        await _repository.SaveChangesAsync(ct);
        return DeliveryResponse.FromDomain(delivery);
    }
}
