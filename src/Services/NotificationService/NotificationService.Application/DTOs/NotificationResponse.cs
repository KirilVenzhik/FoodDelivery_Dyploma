namespace NotificationService.Application.DTOs;

public sealed class NotificationResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public string Message { get; set; } = default!;
    public string Type { get; set; } = default!;
    public DateTime SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
