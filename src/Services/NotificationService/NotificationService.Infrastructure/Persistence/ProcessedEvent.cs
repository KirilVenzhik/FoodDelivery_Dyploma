namespace NotificationService.Infrastructure.Persistence;

public class ProcessedEvent
{
    public Guid CorrelationId { get; set; }
    public string NotificationType { get; set; } = default!;
    public DateTime ProcessedAt { get; set; }
}
