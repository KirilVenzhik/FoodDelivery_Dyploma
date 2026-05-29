using DeliveryService.Application.DTOs;
using DeliveryService.Application.Interfaces;
using DeliveryService.Domain.Entities;

namespace DeliveryService.Application.Commands;

public record CreateDeliveryCommand(Guid OrderId, Guid CorrelationId, Guid ClientId, Guid RestaurantId);

public sealed class CreateDeliveryCommandHandler
{
    private readonly IDeliveryRepository _repository;

    public CreateDeliveryCommandHandler(IDeliveryRepository repository)
        => _repository = repository;

    public async Task<DeliveryResponse> HandleAsync(CreateDeliveryCommand command, CancellationToken ct = default)
    {
        var existing = await _repository.GetByOrderIdAsync(command.OrderId, ct);
        if (existing is not null)
            throw new InvalidOperationException($"Delivery for order '{command.OrderId}' already exists.");

        var delivery = Delivery.Create(command.OrderId, command.CorrelationId, command.ClientId, command.RestaurantId);
        await _repository.AddAsync(delivery, ct);
        await _repository.SaveChangesAsync(ct);
        return DeliveryResponse.FromDomain(delivery);
    }
}
