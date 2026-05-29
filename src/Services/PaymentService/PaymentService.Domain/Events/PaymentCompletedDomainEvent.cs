using Shared.Primitives;

namespace PaymentService.Domain.Events;

public sealed class PaymentCompletedDomainEvent : DomainEvent
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid CorrelationId { get; init; }
    public decimal Amount { get; init; }
}
