using Shared.Primitives;

namespace PaymentService.Domain.Events;

public sealed class PaymentCreatedDomainEvent : DomainEvent
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
}
