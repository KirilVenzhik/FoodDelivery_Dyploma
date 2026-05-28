namespace OrderService.Application.DTOs;

public sealed class OrderItemResponse
{
    public Guid MenuItemId { get; set; }

    public string ItemName { get; set; } = default!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}
