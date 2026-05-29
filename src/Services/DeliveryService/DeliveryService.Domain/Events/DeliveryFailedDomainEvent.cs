using Shared.Primitives;

namespace DeliveryService.Domain.Events;

public sealed class DeliveryFailedDomainEvent : DomainEvent
{
    public Guid DeliveryId { get; }
    public Guid OrderId { get; }
    public string Reason { get; }

    public DeliveryFailedDomainEvent(Guid deliveryId, Guid orderId, string reason)
    {
        DeliveryId = deliveryId;
        OrderId = orderId;
        Reason = reason;
    }
}
