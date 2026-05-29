using DeliveryService.Domain.Entities;

namespace DeliveryService.Application.DTOs;

public sealed class DeliveryResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid ClientId { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid? CourierId { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static DeliveryResponse FromDomain(Delivery d) => new()
    {
        Id = d.Id,
        OrderId = d.OrderId,
        CorrelationId = d.CorrelationId,
        ClientId = d.ClientId,
        RestaurantId = d.RestaurantId,
        CourierId = d.CourierId,
        Status = d.Status.ToString(),
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };
}
