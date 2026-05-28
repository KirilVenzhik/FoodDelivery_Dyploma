using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Application.Queries;

public record GetNotificationsByUserQuery(Guid UserId);

public sealed class GetNotificationsByUserQueryHandler
{
    private readonly INotificationRepository _repository;

    public GetNotificationsByUserQueryHandler(INotificationRepository repository)
        => _repository = repository;

    public async Task<List<NotificationResponse>> HandleAsync(
        GetNotificationsByUserQuery query,
        CancellationToken ct = default)
    {
        var notifications = await _repository.GetByUserIdAsync(query.UserId, ct);

        return notifications.Select(n => new NotificationResponse
        {
            Id = n.Id,
            UserId = n.UserId,
            OrderId = n.OrderId,
            Message = n.Message,
            Type = n.Type,
            SentAt = n.SentAt,
            CreatedAt = n.CreatedAt
        }).ToList();
    }
}
