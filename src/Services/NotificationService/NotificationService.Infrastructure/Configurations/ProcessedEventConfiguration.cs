using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Infrastructure.Configurations;

public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("processed_events");

        builder.HasKey(x => new { x.CorrelationId, x.NotificationType });

        builder.Property(x => x.CorrelationId).HasColumnName("correlation_id");
        builder.Property(x => x.NotificationType).HasColumnName("notification_type").HasMaxLength(50);
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at");
    }
}
