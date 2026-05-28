using PaymentService.Domain.Enums;
using Shared.Primitives;

namespace PaymentService.Domain.Entities;

public class Payment : Entity
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Guid CorrelationId { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid orderId,
        Guid correlationId,
        decimal amount)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CorrelationId = correlationId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Complete()
    {
        Status = PaymentStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = PaymentStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
    }
}
