namespace DeliveryService.Infrastructure.Persistence;

public class ProcessedEvent
{
    public Guid EventId { get; set; }

    public DateTime ProcessedAt { get; set; }
}
