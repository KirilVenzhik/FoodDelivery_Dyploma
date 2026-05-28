namespace OrderService.Application.DTOs;

public sealed class CreateOrderRequest
{
    public Guid RestaurantId { get; set; }

    public List<OrderItemRequest> Items { get; set; } = [];
}
