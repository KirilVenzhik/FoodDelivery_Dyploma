using Shared.Primitives;

namespace DeliveryService.Domain.Events;

public sealed class DeliveryCompletedDomainEvent : DomainEvent
{
    public Guid DeliveryId { get; }
    public Guid OrderId { get; }
    public Guid CorrelationId { get; }
    public Guid CourierId { get; }

    public DeliveryCompletedDomainEvent(Guid deliveryId, Guid orderId, Guid correlationId, Guid courierId)
    {
        DeliveryId = deliveryId;
        OrderId = orderId;
        CorrelationId = correlationId;
        CourierId = courierId;
    }
}
