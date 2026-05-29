using Shared.Primitives;

namespace NotificationService.Domain.Events;

public sealed class NotificationReadDomainEvent : DomainEvent
{
    public Guid NotificationId { get; init; }
    public Guid UserId { get; init; }
}
