using Shared.Primitives;

namespace PaymentService.Domain.Events;

public sealed class PaymentFailedDomainEvent : DomainEvent
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid CorrelationId { get; init; }
    public string Reason { get; init; } = default!;
}
