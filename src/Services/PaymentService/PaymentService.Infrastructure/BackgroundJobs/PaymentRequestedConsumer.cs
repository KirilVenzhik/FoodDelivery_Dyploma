using System.Text;
using System.Text.Json;
using Contracts.Events;
using EventBus.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Application.Commands;
using PaymentService.Domain.Exceptions;
using PaymentService.Infrastructure.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PaymentService.Infrastructure.BackgroundJobs;

public class PaymentRequestedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<PaymentRequestedConsumer> _logger;

    private const string QueueName = "payment-service.payment.requested";
    private const string RoutingKey = "payment.requested";

    public PaymentRequestedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<PaymentRequestedConsumer> logger)
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
            PaymentRequestedEvent? evt = null;

            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                evt = JsonSerializer.Deserialize<PaymentRequestedEvent>(json);

                if (evt is null)
                {
                    channel.BasicNack(ea.DeliveryTag, false, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
                var handler = scope.ServiceProvider.GetRequiredService<ProcessPaymentCommandHandler>();

                await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);

                if (await db.ProcessedEvents.AnyAsync(e => e.EventId == evt.Id, stoppingToken))
                {
                    _logger.LogInformation(
                        "[Saga:{CorrelationId}] Duplicate event ignored. EventId={EventId}",
                        evt.CorrelationId, evt.Id);

                    await transaction.RollbackAsync(stoppingToken);
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var command = new ProcessPaymentCommand(
                    evt.OrderId,
                    evt.CorrelationId,
                    evt.ClientId,
                    evt.Amount);

                await handler.HandleAsync(command, stoppingToken);

                db.ProcessedEvents.Add(new ProcessedEvent
                {
                    EventId = evt.Id,
                    ProcessedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync(stoppingToken);

                await transaction.CommitAsync(stoppingToken);

                _logger.LogInformation(
                    "[Saga:{CorrelationId}] Payment processed. OrderId={OrderId}",
                    evt.CorrelationId, evt.OrderId);

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (DuplicatePaymentException ex)
            {
                _logger.LogWarning(
                    "[Saga:{CorrelationId}] {Message}",
                    evt?.CorrelationId, ex.Message);

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[Saga:{CorrelationId}] Error processing PaymentRequestedEvent. OrderId={OrderId}",
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
