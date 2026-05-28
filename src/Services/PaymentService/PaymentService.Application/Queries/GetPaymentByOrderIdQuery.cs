using PaymentService.Application.Common;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;

namespace PaymentService.Application.Queries;

public record GetPaymentByOrderIdQuery(Guid OrderId);

public sealed class GetPaymentByOrderIdQueryHandler
{
    private readonly IPaymentRepository _repository;

    public GetPaymentByOrderIdQueryHandler(IPaymentRepository repository)
        => _repository = repository;

    public async Task<QueryResult<PaymentResponse>> HandleAsync(
        GetPaymentByOrderIdQuery query,
        CancellationToken ct = default)
    {
        var payment = await _repository.GetByOrderIdAsync(query.OrderId, ct);
        if (payment is null)
            return QueryResult<PaymentResponse>.NotFound();

        return QueryResult<PaymentResponse>.Ok(new PaymentResponse
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            CorrelationId = payment.CorrelationId,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            CreatedAt = payment.CreatedAt,
            ProcessedAt = payment.ProcessedAt
        });
    }
}
