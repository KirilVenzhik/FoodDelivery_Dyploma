using System.Text;
using System.Text.Json;
using Contracts.Events;
using EventBus.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Commands;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Infrastructure.Messaging;

public class NotificationRequestedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<NotificationRequestedConsumer> _logger;

    private const string QueueName = "notification-service.notification.requested";
    private const string RoutingKey = "notification.requested";

    public NotificationRequestedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<NotificationRequestedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            UserName = _options.Username,
            Password = _options.Password
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        channel.QueueBind(
            queue: QueueName,
            exchange: _options.ExchangeName,
            routingKey: RoutingKey);

        channel.BasicQos(0, 1, false);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (_, ea) =>
        {
            NotificationRequestedEvent? evt = null;

            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                evt = JsonSerializer.Deserialize<NotificationRequestedEvent>(json);

                if (evt is null)
                {
                    channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                    return;
                }

                if (!Enum.TryParse<NotificationType>(evt.NotificationType, ignoreCase: true, out var notificationType))
                {
                    _logger.LogWarning("Unknown NotificationType '{Type}' in event {EventId}. Discarding.",
                        evt.NotificationType, evt.Id);
                    channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

                var alreadyProcessed = await db.ProcessedEvents.AnyAsync(
                    e => e.CorrelationId == evt.CorrelationId && e.NotificationType == evt.NotificationType);

                if (alreadyProcessed)
                {
                    _logger.LogInformation(
                        "[Saga:{CorrelationId}] Duplicate notification event '{Type}'. Skipping.",
                        evt.CorrelationId, evt.NotificationType);
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var handler = scope.ServiceProvider.GetRequiredService<CreateNotificationCommandHandler>();

                using var transaction = await db.Database.BeginTransactionAsync();

                await handler.HandleAsync(new CreateNotificationCommand(evt.UserId, evt.OrderId, notificationType));

                db.ProcessedEvents.Add(new ProcessedEvent
                {
                    CorrelationId = evt.CorrelationId,
                    NotificationType = evt.NotificationType,
                    ProcessedAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "[Saga:{CorrelationId}] Notification saved. UserId={UserId}, OrderId={OrderId}, Type={Type}",
                    evt.CorrelationId, evt.UserId, evt.OrderId, evt.NotificationType);

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[Saga:{CorrelationId}] Error processing NotificationRequestedEvent. OrderId={OrderId}",
                    evt?.CorrelationId, evt?.OrderId);

                channel.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
        };

        channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        stoppingToken.Register(() =>
        {
            channel.Close();
            connection.Close();
        });

        return Task.CompletedTask;
    }
}
