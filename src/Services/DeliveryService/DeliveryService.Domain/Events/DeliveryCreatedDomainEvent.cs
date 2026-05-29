using Shared.Primitives;

namespace DeliveryService.Domain.Events;

public sealed class DeliveryCreatedDomainEvent : DomainEvent
{
    public Guid DeliveryId { get; }
    public Guid OrderId { get; }
    public Guid ClientId { get; }

    public DeliveryCreatedDomainEvent(Guid deliveryId, Guid orderId, Guid clientId)
    {
        DeliveryId = deliveryId;
        OrderId = orderId;
        ClientId = clientId;
    }
}
