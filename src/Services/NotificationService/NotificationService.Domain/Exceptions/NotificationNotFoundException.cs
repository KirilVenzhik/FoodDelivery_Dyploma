namespace NotificationService.Domain.Exceptions;

public sealed class NotificationNotFoundException : Exception
{
    public NotificationNotFoundException(Guid id)
        : base($"Notification '{id}' was not found.") { }
}
