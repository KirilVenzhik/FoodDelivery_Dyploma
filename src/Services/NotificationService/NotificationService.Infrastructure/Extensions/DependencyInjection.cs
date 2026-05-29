using EventBus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Commands;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Queries;
using NotificationService.Infrastructure.Messaging;
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

        services.AddScoped<CreateNotificationCommandHandler>();
        services.AddScoped<MarkNotificationReadCommandHandler>();
        services.AddScoped<GetMyNotificationsQueryHandler>();
        services.AddScoped<GetNotificationByIdQueryHandler>();

        services.AddEventBus(configuration);

        services.AddHostedService<NotificationRequestedConsumer>();

        return services;
    }
}
