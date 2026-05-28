using EventBus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Commands;
using OrderService.Application.Interfaces;
using OrderService.Application.Queries;
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

        services.AddScoped<PlaceOrderCommandHandler>();
        services.AddScoped<UpdateOrderStatusCommandHandler>();
        services.AddScoped<GetOrderByIdQueryHandler>();
        services.AddScoped<GetOrdersByClientQueryHandler>();

        services.AddHostedService<OutboxPublisherWorker>();
        services.AddHostedService<PaymentCompletedConsumer>();
        services.AddHostedService<PaymentFailedConsumer>();
        services.AddHostedService<DeliveryCompletedConsumer>();

        return services;
    }
}
