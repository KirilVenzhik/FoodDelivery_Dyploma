using DeliveryService.Application.Commands;
using DeliveryService.Application.Interfaces;
using DeliveryService.Application.Queries;
using DeliveryService.Infrastructure.BackgroundJobs;
using DeliveryService.Infrastructure.Persistence;
using DeliveryService.Infrastructure.Repositories;
using EventBus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DeliveryDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddScoped<IDeliveryRepository, DeliveryRepository>();

        services.AddScoped<AssignDeliveryCommandHandler>();
        services.AddScoped<PickupDeliveryCommandHandler>();
        services.AddScoped<CompleteDeliveryCommandHandler>();
        services.AddScoped<GetDeliveryByIdQueryHandler>();
        services.AddScoped<GetDeliveriesByCourierQueryHandler>();

        services.AddEventBus(configuration);

        services.AddHostedService<DeliveryRequestedConsumer>();
        services.AddHostedService<PaymentCompletedConsumer>();

        return services;
    }
}
