using Shared.Primitives;

namespace OrderService.Domain.Events;

public sealed class OrderPaidDomainEvent : DomainEvent
{
    public Guid OrderId { get; init; }
    public Guid ClientId { get; init; }
}
