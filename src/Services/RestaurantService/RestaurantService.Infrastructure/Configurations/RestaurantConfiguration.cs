using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantService.Domain.Entities;

namespace RestaurantService.Infrastructure.Persistence.Configurations;

public class RestaurantConfiguration
    : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(
        EntityTypeBuilder<Restaurant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200);

        builder
            .HasMany(r => r.MenuItems)
            .WithOne()
            .HasForeignKey(m => m.RestaurantId);

        builder.Navigation(r => r.MenuItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Restaurant.MenuItems))!
            .SetField("_menuItems");
    }
}