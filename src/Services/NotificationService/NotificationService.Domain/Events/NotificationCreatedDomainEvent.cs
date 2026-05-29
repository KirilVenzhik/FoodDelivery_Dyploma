using NotificationService.Domain.Enums;
using Shared.Primitives;

namespace NotificationService.Domain.Events;

public sealed class NotificationCreatedDomainEvent : DomainEvent
{
    public Guid NotificationId { get; init; }
    public Guid UserId { get; init; }
    public Guid OrderId { get; init; }
    public NotificationType Type { get; init; }
}
