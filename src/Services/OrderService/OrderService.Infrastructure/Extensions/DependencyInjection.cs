using EventBus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.BackgroundJobs;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddHttpClient<IRestaurantServiceClient, RestaurantServiceHttpClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["RestaurantService:BaseUrl"]!);
        });

        services.AddEventBus(configuration);

        services.AddHostedService<OutboxPublisherWorker>();
        services.AddHostedService<PaymentCompletedConsumer>();
        services.AddHostedService<PaymentFailedConsumer>();

        return services;
    }
}
