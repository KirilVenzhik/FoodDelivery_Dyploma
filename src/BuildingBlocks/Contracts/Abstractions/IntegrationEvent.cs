namespace Contracts.Abstractions;

public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;

    public Guid CorrelationId { get; init; }
}
