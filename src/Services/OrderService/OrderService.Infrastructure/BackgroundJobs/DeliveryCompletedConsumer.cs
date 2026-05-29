using System.Text;
using System.Text.Json;
using Contracts.Events;
using EventBus.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderService.Application.Interfaces;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderService.Infrastructure.BackgroundJobs;

public class DeliveryCompletedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<DeliveryCompletedConsumer> _logger;

    private const string QueueName = "order-service.delivery.completed";
    private const string RoutingKey = "delivery.completed";

    public DeliveryCompletedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<DeliveryCompletedConsumer> logger)
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

        channel.ExchangeDeclare(exchange: _options.ExchangeName, type: ExchangeType.Topic, durable: true);
        channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue: QueueName, exchange: _options.ExchangeName, routingKey: RoutingKey);
        channel.BasicQos(0, 1, false);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (_, ea) =>
        {
            DeliveryCompletedEvent? evt = null;

            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                evt = JsonSerializer.Deserialize<DeliveryCompletedEvent>(json);

                if (evt is null)
                {
                    channel.BasicNack(ea.DeliveryTag, false, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                if (await db.ProcessedEvents.AnyAsync(e => e.EventId == evt.Id))
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == evt.OrderId);

                if (order is not null)
                {
                    if (order.Status == OrderStatus.ReadyForDelivery)
                        order.ChangeStatus(OrderStatus.InDelivery);
                    order.ChangeStatus(OrderStatus.Delivered);

                    var outbox = scope.ServiceProvider.GetRequiredService<IOutboxService>();
                    await outbox.SaveMessageAsync(new NotificationRequestedEvent
                    {
                        CorrelationId = evt.CorrelationId,
                        UserId = order.ClientId,
                        OrderId = order.Id,
                        Message = "Your order has been delivered.",
                        NotificationType = "OrderDelivered"
                    }, "notification.requested");
                }

                db.ProcessedEvents.Add(new ProcessedEvent { EventId = evt.Id, ProcessedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();

                _logger.LogInformation(
                    "[Saga:{CorrelationId}] Delivery completed. OrderId={OrderId}, DeliveryId={DeliveryId}",
                    evt.CorrelationId, evt.OrderId, evt.DeliveryId);

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[Saga:{CorrelationId}] Error processing DeliveryCompletedEvent. OrderId={OrderId}",
                    evt?.CorrelationId, evt?.OrderId);

                channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        stoppingToken.Register(() =>
        {
            channel.Close();
            connection.Close();
        });

        return Task.CompletedTask;
    }
}
