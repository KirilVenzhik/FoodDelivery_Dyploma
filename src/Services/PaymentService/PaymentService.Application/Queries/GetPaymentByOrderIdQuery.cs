using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Exceptions;

namespace PaymentService.Application.Queries;

public record GetPaymentByOrderIdQuery(
    Guid OrderId,
    Guid RequestingClientId,
    string RequestingRole);

public sealed class GetPaymentByOrderIdQueryHandler
{
    private readonly IPaymentRepository _repository;

    public GetPaymentByOrderIdQueryHandler(IPaymentRepository repository)
        => _repository = repository;

    public async Task<PaymentResponse> HandleAsync(
        GetPaymentByOrderIdQuery query,
        CancellationToken ct = default)
    {
        var payment = await _repository.GetByOrderIdAsync(query.OrderId, ct);
        if (payment is null)
            throw new PaymentNotFoundException(query.OrderId);

        if (query.RequestingRole == "Customer" && payment.ClientId != query.RequestingClientId)
            throw new UnauthorizedAccessException(
                "You are not authorized to view this payment.");

        return new PaymentResponse
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
}
