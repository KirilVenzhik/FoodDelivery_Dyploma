using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.Application.Queries;

public record GetOrdersByClientQuery(Guid ClientId);

public sealed class GetOrdersByClientQueryHandler
{
    private readonly IOrderRepository _repository;

    public GetOrdersByClientQueryHandler(IOrderRepository repository)
        => _repository = repository;

    public async Task<List<OrderResponse>> HandleAsync(GetOrdersByClientQuery query, CancellationToken ct = default)
    {
        var orders = await _repository.GetByClientIdAsync(query.ClientId, ct);
        return orders.Select(GetOrderByIdQueryHandler.MapToResponse).ToList();
    }
}
