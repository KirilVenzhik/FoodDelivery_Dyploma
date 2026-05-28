using System.Text.Json;
using Contracts.Events;
using EventBus.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.BackgroundJobs;

public class OutboxPublisherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OutboxPublisherWorker> _logger;

    public OutboxPublisherWorker(
        IServiceScopeFactory scopeFactory,
        IEventBus eventBus,
        ILogger<OutboxPublisherWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Outbox] Error processing outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        var messages = await db.OutboxMessages
            .Where(m => !m.Processed)
            .OrderBy(m => m.OccurredOnUtc)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            await PublishMessageAsync(message, ct);
            message.Processed = true;
            message.ProcessedOnUtc = DateTime.UtcNow;
            _logger.LogInformation("[Outbox] Published {Type}", message.Type);
        }

        if (messages.Count > 0)
        {
            await db.SaveChangesAsync(ct);
        }
    }

    private async Task PublishMessageAsync(Shared.Primitives.OutboxMessage message, CancellationToken ct)
    {
        switch (message.Type)
        {
            case nameof(PaymentRequestedEvent):
                var paymentRequested = JsonSerializer.Deserialize<PaymentRequestedEvent>(message.Payload)!;
                await _eventBus.PublishAsync(paymentRequested, "payment.requested", ct);
                break;

            case nameof(NotificationRequestedEvent):
                var notificationRequested = JsonSerializer.Deserialize<NotificationRequestedEvent>(message.Payload)!;
                await _eventBus.PublishAsync(notificationRequested, "notification.requested", ct);
                break;

            case nameof(DeliveryRequestedEvent):
                var deliveryRequested = JsonSerializer.Deserialize<DeliveryRequestedEvent>(message.Payload)!;
                await _eventBus.PublishAsync(deliveryRequested, "delivery.requested", ct);
                break;

            default:
                _logger.LogWarning("[Outbox] Unknown message type: {Type}", message.Type);
                break;
        }
    }
}
