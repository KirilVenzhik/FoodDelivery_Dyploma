using System.Text;
using System.Text.Json;
using Contracts.Abstractions;
using EventBus.Abstractions;
using EventBus.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EventBus.RabbitMq;

public class RabbitMqEventBus : IEventBus
{
    private readonly RabbitMqOptions _options;

    public RabbitMqEventBus(
        IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public Task PublishAsync<T>(
        T integrationEvent,
        string routingKey,
        CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            UserName = _options.Username,
            Password = _options.Password
        };

        using var connection = factory.CreateConnection();

        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(integrationEvent));

        var properties = channel.CreateBasicProperties();

        properties.Persistent = true;

        channel.BasicPublish(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);

        return Task.CompletedTask;
    }
}