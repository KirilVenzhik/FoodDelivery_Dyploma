using EventBus.Abstractions;
using EventBus.Options;
using EventBus.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Extensions;

public static class EventBusExtensions
{
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));

        services.AddSingleton<IEventBus, RabbitMqEventBus>();

        return services;
    }
}