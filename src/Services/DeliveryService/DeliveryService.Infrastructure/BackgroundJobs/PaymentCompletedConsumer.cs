using System.Text;
using System.Text.Json;
using Contracts.Events;
using DeliveryService.Application.Commands;
using DeliveryService.Infrastructure.Persistence;
using EventBus.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DeliveryService.Infrastructure.BackgroundJobs;

public class PaymentCompletedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<PaymentCompletedConsumer> _logger;

    private const string QueueName = "delivery-service.payment.completed";
    private const string RoutingKey = "payment.completed";

    public PaymentCompletedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<PaymentCompletedConsumer> logger)
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
            PaymentCompletedEvent? evt = null;

            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                evt = JsonSerializer.Deserialize<PaymentCompletedEvent>(json);

                if (evt is null)
                {
                    channel.BasicNack(ea.DeliveryTag, false, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();

                if (await db.ProcessedEvents.AnyAsync(e => e.EventId == evt.CorrelationId))
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var createHandler = scope.ServiceProvider.GetRequiredService<CreateDeliveryCommandHandler>();

                await using var transaction = await db.Database.BeginTransactionAsync();

                await createHandler.HandleAsync(
                    new CreateDeliveryCommand(evt.OrderId, evt.CorrelationId, evt.ClientId, evt.RestaurantId));

                db.ProcessedEvents.Add(new ProcessedEvent
                {
                    EventId = evt.CorrelationId,
                    ProcessedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "[Saga:{CorrelationId}] Delivery created. OrderId={OrderId}",
                    evt.CorrelationId, evt.OrderId);

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[Saga:{CorrelationId}] Error processing PaymentCompletedEvent. OrderId={OrderId}",
                    evt?.CorrelationId, evt?.OrderId);

                channel.BasicNack(ea.DeliveryTag, false, true);
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
