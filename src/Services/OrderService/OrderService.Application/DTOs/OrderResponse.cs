namespace OrderService.Application.DTOs;

public sealed class OrderResponse
{
    public Guid Id { get; set; }

    public Guid CorrelationId { get; set; }

    public Guid ClientId { get; set; }

    public Guid RestaurantId { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; }

    public List<OrderItemResponse> Items { get; set; } = [];
}
