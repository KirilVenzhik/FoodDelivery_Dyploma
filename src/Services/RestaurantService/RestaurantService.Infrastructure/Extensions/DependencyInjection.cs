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
            options.UseNpgsql(
                configuration.GetConnectionString("Database"));
        });

        services.AddScoped<IRestaurantRepository, RestaurantRepository>();

        services.AddScoped<CreateRestaurantCommandHandler>();
        services.AddScoped<AddMenuItemCommandHandler>();
        services.AddScoped<UpdateMenuItemCommandHandler>();
        services.AddScoped<DeleteMenuItemCommandHandler>();
        services.AddScoped<OpenRestaurantCommandHandler>();
        services.AddScoped<CloseRestaurantCommandHandler>();

        services.AddScoped<GetAllRestaurantsQueryHandler>();
        services.AddScoped<GetRestaurantByIdQueryHandler>();
        services.AddScoped<GetMenuItemsQueryHandler>();

        return services;
    }
}
