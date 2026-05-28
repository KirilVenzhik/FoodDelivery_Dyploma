using Contracts.Abstractions;

namespace Contracts.Events;

public class PaymentCompletedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }

    public Guid PaymentId { get; init; }

    public decimal Amount { get; init; }
}
