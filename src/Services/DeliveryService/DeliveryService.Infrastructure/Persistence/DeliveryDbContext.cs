using DeliveryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Infrastructure.Persistence;

public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options)
        : base(options) { }

    public DbSet<Delivery> Deliveries => Set<Delivery>();

    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeliveryDbContext).Assembly);
    }
}
