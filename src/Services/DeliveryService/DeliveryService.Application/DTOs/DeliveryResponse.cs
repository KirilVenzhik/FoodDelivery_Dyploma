using DeliveryService.Domain.Entities;

namespace DeliveryService.Application.DTOs;

public sealed class DeliveryResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? CourierId { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static DeliveryResponse FromDomain(Delivery d) => new()
    {
        Id = d.Id,
        OrderId = d.OrderId,
        CourierId = d.CourierId,
        Status = d.Status.ToString(),
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };
}
