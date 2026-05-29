using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Application.Commands;

public record CreateNotificationCommand(Guid UserId, Guid OrderId, NotificationType Type);

public sealed class CreateNotificationCommandHandler
{
    private readonly INotificationRepository _repository;

    public CreateNotificationCommandHandler(INotificationRepository repository)
        => _repository = repository;

    public async Task<NotificationResponse> HandleAsync(
        CreateNotificationCommand command,
        CancellationToken ct = default)
    {
        var message = command.Type switch
        {
            NotificationType.OrderPlaced    => NotificationMessage.ForOrderPlaced(command.OrderId),
            NotificationType.PaymentSuccess => NotificationMessage.ForPaymentSuccess(command.OrderId),
            NotificationType.PaymentFailed  => NotificationMessage.ForPaymentFailed(command.OrderId),
            NotificationType.OrderDelivered => NotificationMessage.ForOrderDelivered(command.OrderId),
            _ => throw new ArgumentOutOfRangeException(nameof(command.Type), command.Type, null)
        };

        var notification = Notification.Create(command.UserId, command.OrderId, message, command.Type);
        notification.MarkAsSent();

        await _repository.AddAsync(notification, ct);
        await _repository.SaveChangesAsync(ct);

        return ToResponse(notification);
    }

    internal static NotificationResponse ToResponse(Notification n) => new()
    {
        Id        = n.Id,
        UserId    = n.UserId,
        OrderId   = n.OrderId,
        Message   = n.Message,
        Type      = n.Type.ToString(),
        IsRead    = n.IsRead,
        CreatedAt = n.CreatedAt,
        SentAt    = n.SentAt
    };
}
