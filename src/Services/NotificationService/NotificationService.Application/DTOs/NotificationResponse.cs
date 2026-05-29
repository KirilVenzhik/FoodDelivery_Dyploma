namespace NotificationService.Application.DTOs;

public sealed class NotificationResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid OrderId { get; init; }
    public string Message { get; init; } = default!;
    public string Type { get; init; } = default!;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? SentAt { get; init; }
}
