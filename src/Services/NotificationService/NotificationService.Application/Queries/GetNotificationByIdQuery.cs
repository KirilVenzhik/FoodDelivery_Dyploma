using NotificationService.Application.Commands;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Exceptions;

namespace NotificationService.Application.Queries;

public record GetNotificationByIdQuery(Guid NotificationId, Guid RequestingUserId);

public sealed class GetNotificationByIdQueryHandler
{
    private readonly INotificationRepository _repository;

    public GetNotificationByIdQueryHandler(INotificationRepository repository)
        => _repository = repository;

    public async Task<NotificationResponse> HandleAsync(
        GetNotificationByIdQuery query,
        CancellationToken ct = default)
    {
        var notification = await _repository.GetByIdAsync(query.NotificationId, ct)
            ?? throw new NotificationNotFoundException(query.NotificationId);

        if (notification.UserId != query.RequestingUserId)
            throw new UnauthorizedAccessException("You do not have access to this notification.");

        return CreateNotificationCommandHandler.ToResponse(notification);
    }
}
