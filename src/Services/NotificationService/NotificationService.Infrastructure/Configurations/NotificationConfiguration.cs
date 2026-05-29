using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").IsRequired().HasColumnType("text");
        builder.Property(x => x.Type).HasColumnName("type").HasConversion<int>();
        builder.Property(x => x.IsRead).HasColumnName("is_read").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.SentAt).HasColumnName("sent_at");
    }
}
