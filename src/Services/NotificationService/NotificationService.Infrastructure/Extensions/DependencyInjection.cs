using EventBus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.BackgroundJobs;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Repositories;

namespace NotificationService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddEventBus(configuration);

        services.AddHostedService<NotificationRequestedConsumer>();

        return services;
    }
}
