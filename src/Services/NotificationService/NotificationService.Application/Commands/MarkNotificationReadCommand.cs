using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Exceptions;

namespace NotificationService.Application.Commands;

public record MarkNotificationReadCommand(Guid NotificationId, Guid RequestingUserId);

public sealed class MarkNotificationReadCommandHandler
{
    private readonly INotificationRepository _repository;

    public MarkNotificationReadCommandHandler(INotificationRepository repository)
        => _repository = repository;

    public async Task<NotificationResponse> HandleAsync(
        MarkNotificationReadCommand command,
        CancellationToken ct = default)
    {
        var notification = await _repository.GetByIdAsync(command.NotificationId, ct)
            ?? throw new NotificationNotFoundException(command.NotificationId);

        if (notification.UserId != command.RequestingUserId)
            throw new UnauthorizedAccessException("You do not have access to this notification.");

        notification.MarkAsRead();
        await _repository.SaveChangesAsync(ct);

        return CreateNotificationCommandHandler.ToResponse(notification);
    }
}
