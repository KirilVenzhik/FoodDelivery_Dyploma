using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantService.Application.Interfaces;
using RestaurantService.Infrastructure.Persistence;
using RestaurantService.Infrastructure.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<RestaurantDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("Database"));
        });

        services.AddScoped<IRestaurantRepository, RestaurantRepository>();

        return services;
    }
}
