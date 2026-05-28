using Contracts.Abstractions;

namespace Contracts.Events;

public class PaymentFailedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }

    public string Reason { get; init; } = default!;
}
