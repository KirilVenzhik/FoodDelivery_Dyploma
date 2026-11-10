using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantService.Domain.Entities;

namespace RestaurantService.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("menu_items");

        builder.HasKey(x => x.Id);
        // Id is assigned by the domain (MenuItem.Create). Tell EF it is app-generated so a new
        // item added to a tracked restaurant's collection is INSERTed, not mistaken for an UPDATE.
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.RestaurantId).HasColumnName("restaurant_id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Price).HasColumnName("price").HasColumnType("decimal(18,2)");
        builder.Property(x => x.Available).HasColumnName("available");
    }
}
