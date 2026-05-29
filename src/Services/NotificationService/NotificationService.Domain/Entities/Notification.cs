using NotificationService.Domain.Enums;
using NotificationService.Domain.Events;
using Shared.Primitives;

namespace NotificationService.Domain.Entities;

public class Notification : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrderId { get; private set; }
    public string Message { get; private set; } = default!;
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    private Notification() { }

    public static Notification Create(Guid userId, Guid orderId, string message, NotificationType type)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderId = orderId,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        notification.RaiseDomainEvent(new NotificationCreatedDomainEvent
        {
            NotificationId = notification.Id,
            UserId = notification.UserId,
            OrderId = notification.OrderId,
            Type = notification.Type
        });

        return notification;
    }

    public void MarkAsSent()
    {
        SentAt = DateTime.UtcNow;

        RaiseDomainEvent(new NotificationSentDomainEvent
        {
            NotificationId = Id,
            UserId = UserId
        });
    }

    public void MarkAsRead()
    {
        IsRead = true;

        RaiseDomainEvent(new NotificationReadDomainEvent
        {
            NotificationId = Id,
            UserId = UserId
        });
    }
}
