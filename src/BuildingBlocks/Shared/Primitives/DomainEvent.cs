using Shared.Abstractions;

namespace Shared.Primitives;

public class DomainEvent : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
