using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantService.Application.Commands;
using RestaurantService.Application.Interfaces;
using RestaurantService.Application.Queries;
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
            options.UseNpgsql(configuration.GetConnectionString("Database"));
        });

        services.AddScoped<IRestaurantRepository, RestaurantRepository>();

        services.AddScoped<CreateRestaurantCommandHandler>();
        services.AddScoped<UpdateRestaurantCommandHandler>();
        services.AddScoped<AddMenuItemCommandHandler>();
        services.AddScoped<UpdateMenuItemCommandHandler>();
        services.AddScoped<RemoveMenuItemCommandHandler>();
        services.AddScoped<ChangeRestaurantAvailabilityCommandHandler>();

        services.AddScoped<GetAllRestaurantsQueryHandler>();
        services.AddScoped<GetRestaurantByIdQueryHandler>();
        services.AddScoped<GetMyRestaurantsQueryHandler>();

        return services;
    }
}
