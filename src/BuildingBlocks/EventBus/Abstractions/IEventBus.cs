using Contracts.Abstractions;

namespace EventBus.Abstractions;

public interface IEventBus
{
    Task PublishAsync<T>(
        T integrationEvent,
        string routingKey,
        CancellationToken cancellationToken = default)
        where T : IIntegrationEvent;
}