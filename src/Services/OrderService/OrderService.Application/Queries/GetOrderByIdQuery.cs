using OrderService.Application.Common;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.Queries;

public record GetOrderByIdQuery(Guid OrderId, Guid RequesterId, string RequesterRole);

public sealed class GetOrderByIdQueryHandler
{
    private readonly IOrderRepository _repository;

    public GetOrderByIdQueryHandler(IOrderRepository repository)
        => _repository = repository;

    public async Task<QueryResult<OrderResponse>> HandleAsync(GetOrderByIdQuery query, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(query.OrderId, ct);
        if (order is null)
            return QueryResult<OrderResponse>.NotFound();

        if (order.ClientId != query.RequesterId && query.RequesterRole != "Administrator")
            return QueryResult<OrderResponse>.Forbidden();

        return QueryResult<OrderResponse>.Ok(MapToResponse(order));
    }

    internal static OrderResponse MapToResponse(Order order) => new()
    {
        Id = order.Id,
        CorrelationId = order.CorrelationId,
        ClientId = order.ClientId,
        RestaurantId = order.RestaurantId,
        TotalAmount = order.TotalAmount,
        Status = order.Status.ToString(),
        CreatedAtUtc = order.CreatedAtUtc,
        Items = order.Items.Select(i => new OrderItemResponse
        {
            MenuItemId = i.MenuItemId,
            ItemName = i.ItemName,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            LineTotal = i.LineTotal
        }).ToList()
    };
}
