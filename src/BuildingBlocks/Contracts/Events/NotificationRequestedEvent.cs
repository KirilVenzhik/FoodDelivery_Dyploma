using Contracts.Abstractions;

namespace Contracts.Events;

public class NotificationRequestedEvent : IntegrationEvent
{
    public Guid UserId { get; init; }

    public Guid OrderId { get; init; }

    public string Message { get; init; } = default!;

    public string NotificationType { get; init; } = default!;
}
