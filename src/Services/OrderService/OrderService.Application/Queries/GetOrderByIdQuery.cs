using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mapping;
using OrderService.Domain.Exceptions;

namespace OrderService.Application.Queries;

public record GetOrderByIdQuery(Guid OrderId, Guid RequesterId, string RequesterRole);

public sealed class GetOrderByIdQueryHandler
{
    private readonly IOrderRepository _repository;

    public GetOrderByIdQueryHandler(IOrderRepository repository)
        => _repository = repository;

    public async Task<OrderResponse> HandleAsync(GetOrderByIdQuery query, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(query.OrderId, ct)
            ?? throw new OrderNotFoundException(query.OrderId);

        if (order.ClientId != query.RequesterId && query.RequesterRole != "Administrator")
            throw new UnauthorizedAccessException("You do not have access to this order.");

        return OrderMapper.MapToResponse(order);
    }
}
