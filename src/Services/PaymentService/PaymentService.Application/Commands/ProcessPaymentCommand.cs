using Contracts.Events;
using EventBus.Abstractions;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Exceptions;

namespace PaymentService.Application.Commands;

public record ProcessPaymentCommand(
    Guid OrderId,
    Guid CorrelationId,
    Guid ClientId,
    decimal Amount);

public sealed class ProcessPaymentCommandHandler
{
    private readonly IPaymentRepository _repository;
    private readonly IEventBus _eventBus;

    public ProcessPaymentCommandHandler(
        IPaymentRepository repository,
        IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task<PaymentResponse> HandleAsync(
        ProcessPaymentCommand command,
        CancellationToken ct = default)
    {
        var existing = await _repository.GetByOrderIdAsync(command.OrderId, ct);
        if (existing is not null)
            throw new DuplicatePaymentException(command.OrderId);

        var payment = Payment.Create(
            command.OrderId,
            command.CorrelationId,
            command.ClientId,
            command.Amount);

        await _repository.AddAsync(payment, ct);
        await _repository.SaveChangesAsync(ct);

        payment.Complete();
        await _repository.SaveChangesAsync(ct);

        await _eventBus.PublishAsync(
            new PaymentCompletedEvent
            {
                OrderId = payment.OrderId,
                PaymentId = payment.Id,
                CorrelationId = payment.CorrelationId,
                Amount = payment.Amount
            },
            "payment.completed",
            ct);

        return MapToResponse(payment);
    }

    private static PaymentResponse MapToResponse(Payment payment) => new()
    {
        Id = payment.Id,
        OrderId = payment.OrderId,
        CorrelationId = payment.CorrelationId,
        ClientId = payment.ClientId,
        Amount = payment.Amount,
        Status = payment.Status.ToString(),
        CreatedAt = payment.CreatedAt,
        ProcessedAt = payment.ProcessedAt
    };
}
