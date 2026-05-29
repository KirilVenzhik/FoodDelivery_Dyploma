using Shared.Primitives;

namespace OrderService.Domain.Events;

public sealed class OrderPaymentFailedDomainEvent : DomainEvent
{
    public Guid OrderId { get; init; }
    public Guid ClientId { get; init; }
    public string Reason { get; init; } = default!;
}
