using Shared.Primitives;

namespace OrderService.Domain.Events;

public sealed class OrderCreatedDomainEvent : DomainEvent
{
    public Guid OrderId { get; init; }
    public Guid ClientId { get; init; }
    public Guid RestaurantId { get; init; }
    public decimal TotalAmount { get; init; }
}
