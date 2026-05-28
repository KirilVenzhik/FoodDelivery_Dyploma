using Microsoft.EntityFrameworkCore;
using RestaurantService.Domain.Entities;

namespace RestaurantService.Infrastructure.Persistence;

public class RestaurantDbContext : DbContext
{
    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
        : base(options) { }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();

    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RestaurantDbContext).Assembly);
    }
}