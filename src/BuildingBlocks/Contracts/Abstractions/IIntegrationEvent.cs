namespace Contracts.Abstractions;

public interface IIntegrationEvent
{
    Guid Id { get; }

    DateTime OccurredOnUtc { get; }

    Guid CorrelationId { get; }
}
