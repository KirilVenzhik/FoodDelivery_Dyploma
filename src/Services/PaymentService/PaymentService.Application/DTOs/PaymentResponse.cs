namespace PaymentService.Application.DTOs;

public sealed class PaymentResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid ClientId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
