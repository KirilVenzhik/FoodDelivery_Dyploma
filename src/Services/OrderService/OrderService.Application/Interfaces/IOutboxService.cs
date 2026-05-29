using Contracts.Abstractions;

namespace OrderService.Application.Interfaces;

public interface IOutboxService
{
    Task SaveMessageAsync<T>(T integrationEvent, string routingKey, CancellationToken ct = default)
        where T : IIntegrationEvent;
}
