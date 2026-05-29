using Shared.Primitives;

namespace NotificationService.Domain.Events;

public sealed class NotificationSentDomainEvent : DomainEvent
{
    public Guid NotificationId { get; init; }
    public Guid UserId { get; init; }
}
