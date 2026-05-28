using Contracts.Abstractions;

namespace Contracts.Events;

public class DeliveryCompletedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }

    public Guid DeliveryId { get; init; }
}
