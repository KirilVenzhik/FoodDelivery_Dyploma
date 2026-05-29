using Shared.Primitives;

namespace DeliveryService.Domain.Events;

public sealed class DeliveryPickedUpDomainEvent : DomainEvent
{
    public Guid DeliveryId { get; }
    public Guid OrderId { get; }
    public Guid CourierId { get; }

    public DeliveryPickedUpDomainEvent(Guid deliveryId, Guid orderId, Guid courierId)
    {
        DeliveryId = deliveryId;
        OrderId = orderId;
        CourierId = courierId;
    }
}
