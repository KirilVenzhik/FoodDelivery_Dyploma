using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mapping;

internal static class OrderMapper
{
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
