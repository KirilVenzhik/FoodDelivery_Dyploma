using Contracts.Abstractions;

namespace Contracts.Events;

public class PaymentRequestedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }

    public Guid ClientId { get; init; }

    public Guid RestaurantId { get; init; }

    public decimal Amount { get; init; }
}
