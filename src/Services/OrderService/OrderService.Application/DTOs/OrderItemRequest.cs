namespace OrderService.Application.DTOs;

public sealed class OrderItemRequest
{
    public Guid MenuItemId { get; set; }

    public int Quantity { get; set; }
}
