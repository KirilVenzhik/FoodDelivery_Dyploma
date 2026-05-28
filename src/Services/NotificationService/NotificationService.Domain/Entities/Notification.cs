using Shared.Primitives;

namespace NotificationService.Domain.Entities;

public class Notification : Entity
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid OrderId { get; private set; }

    public string Message { get; private set; } = default!;

    public string Type { get; private set; } = default!;

    public DateTime SentAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid userId,
        Guid orderId,
        string message,
        string type)
    {
        var now = DateTime.UtcNow;

        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderId = orderId,
            Message = message,
            Type = type,
            SentAt = now,
            CreatedAt = now
        };
    }
}
