using EventBus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Queries;
using PaymentService.Infrastructure.BackgroundJobs;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.Repositories;

namespace PaymentService.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<GetPaymentByOrderIdQueryHandler>();

        services.AddEventBus(configuration);

        services.AddHostedService<PaymentRequestedConsumer>();

        return services;
    }
}
