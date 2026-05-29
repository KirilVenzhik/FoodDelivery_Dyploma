using System.Text.Json;
using Contracts.Abstractions;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Persistence;
using Shared.Primitives;

namespace OrderService.Infrastructure.Services;

public sealed class OutboxService : IOutboxService
{
    private readonly OrderDbContext _dbContext;

    public OutboxService(OrderDbContext dbContext)
        => _dbContext = dbContext;

    public Task SaveMessageAsync<T>(T integrationEvent, string routingKey, CancellationToken ct = default)
        where T : IIntegrationEvent
    {
        _dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(T).Name,
            Payload = JsonSerializer.Serialize(integrationEvent),
            OccurredOnUtc = DateTime.UtcNow
        });

        return Task.CompletedTask;
    }
}
