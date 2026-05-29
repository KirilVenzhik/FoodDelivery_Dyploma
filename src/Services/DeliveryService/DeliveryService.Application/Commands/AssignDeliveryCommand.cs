using DeliveryService.Application.DTOs;
using DeliveryService.Application.Interfaces;
using DeliveryService.Domain.Enums;
using DeliveryService.Domain.Exceptions;

namespace DeliveryService.Application.Commands;

public record AssignDeliveryCommand(Guid DeliveryId, Guid CourierId);

public sealed class AssignDeliveryCommandHandler
{
    private readonly IDeliveryRepository _repository;

    public AssignDeliveryCommandHandler(IDeliveryRepository repository)
        => _repository = repository;

    public async Task<DeliveryResponse> HandleAsync(AssignDeliveryCommand command, CancellationToken ct = default)
    {
        var delivery = await _repository.GetByIdAsync(command.DeliveryId, ct)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        if (delivery.Status != DeliveryStatus.Pending)
            throw new DeliveryAlreadyAssignedException(command.DeliveryId);

        delivery.Assign(command.CourierId);
        await _repository.SaveChangesAsync(ct);
        return DeliveryResponse.FromDomain(delivery);
    }
}
