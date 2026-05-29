using NotificationService.Application.Commands;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Queries;

public record GetMyNotificationsQuery(Guid UserId);

public sealed class GetMyNotificationsQueryHandler
{
    private readonly INotificationRepository _repository;

    public GetMyNotificationsQueryHandler(INotificationRepository repository)
        => _repository = repository;

    public async Task<List<NotificationResponse>> HandleAsync(
        GetMyNotificationsQuery query,
        CancellationToken ct = default)
    {
        var notifications = await _repository.GetByUserIdAsync(query.UserId, ct);

        return notifications
            .Select(CreateNotificationCommandHandler.ToResponse)
            .ToList();
    }
}
