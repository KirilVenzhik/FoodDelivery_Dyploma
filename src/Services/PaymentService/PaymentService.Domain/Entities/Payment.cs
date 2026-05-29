using PaymentService.Domain.Enums;
using PaymentService.Domain.Events;
using PaymentService.Domain.Exceptions;
using Shared.Primitives;

namespace PaymentService.Domain.Entities;

public class Payment : Entity
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CorrelationId { get; private set; }
    public Guid ClientId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid orderId,
        Guid correlationId,
        Guid clientId,
        decimal amount)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CorrelationId = correlationId,
            ClientId = clientId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        payment.RaiseDomainEvent(new PaymentCreatedDomainEvent
        {
            PaymentId = payment.Id,
            OrderId = orderId,
            Amount = amount
        });

        return payment;
    }

    public void Complete()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidPaymentOperationException(
                $"Cannot complete a payment that is already in '{Status}' status.");

        Status = PaymentStatus.Completed;
        ProcessedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PaymentCompletedDomainEvent
        {
            PaymentId = Id,
            OrderId = OrderId,
            CorrelationId = CorrelationId,
            Amount = Amount
        });
    }

    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidPaymentOperationException(
                $"Cannot fail a payment that is already in '{Status}' status.");

        Status = PaymentStatus.Failed;
        ProcessedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PaymentFailedDomainEvent
        {
            PaymentId = Id,
            OrderId = OrderId,
            CorrelationId = CorrelationId,
            Reason = reason
        });
    }
}
