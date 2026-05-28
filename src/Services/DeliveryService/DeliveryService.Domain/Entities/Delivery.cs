using DeliveryService.Domain.Enums;
using Shared.Primitives;

namespace DeliveryService.Domain.Entities;

public class Delivery : Entity
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Guid? CourierId { get; private set; }

    public DeliveryStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private Delivery() { }

    public static Delivery Create(Guid orderId)
    {
        var now = DateTime.UtcNow;

        return new Delivery
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Status = DeliveryStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Assign(Guid courierId)
    {
        CourierId = courierId;
        Status = DeliveryStatus.Assigned;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pickup()
    {
        Status = DeliveryStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = DeliveryStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = DeliveryStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }
}
