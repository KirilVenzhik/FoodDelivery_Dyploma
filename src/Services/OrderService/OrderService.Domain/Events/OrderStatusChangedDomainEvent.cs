using OrderService.Domain.Enums;
using Shared.Primitives;

namespace OrderService.Domain.Events;

public sealed class OrderStatusChangedDomainEvent : DomainEvent
{
    public Guid OrderId { get; init; }
    public OrderStatus OldStatus { get; init; }
    public OrderStatus NewStatus { get; init; }
}
