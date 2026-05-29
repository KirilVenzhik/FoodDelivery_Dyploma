using Shared.Primitives;

namespace DeliveryService.Domain.Events;

public sealed class DeliveryAssignedDomainEvent : DomainEvent
{
    public Guid DeliveryId { get; }
    public Guid OrderId { get; }
    public Guid CourierId { get; }

    public DeliveryAssignedDomainEvent(Guid deliveryId, Guid orderId, Guid courierId)
    {
        DeliveryId = deliveryId;
        OrderId = orderId;
        CourierId = courierId;
    }
}
