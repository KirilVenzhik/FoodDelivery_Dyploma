using DeliveryService.Domain.Enums;
using DeliveryService.Domain.Events;
using DeliveryService.Domain.Exceptions;
using Shared.Primitives;

namespace DeliveryService.Domain.Entities;

public class Delivery : Entity
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CorrelationId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid RestaurantId { get; private set; }
    public Guid? CourierId { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Delivery() { }

    public static Delivery Create(Guid orderId, Guid correlationId, Guid clientId, Guid restaurantId)
    {
        var now = DateTime.UtcNow;
        var delivery = new Delivery
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CorrelationId = correlationId,
            ClientId = clientId,
            RestaurantId = restaurantId,
            Status = DeliveryStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
        delivery.RaiseDomainEvent(new DeliveryCreatedDomainEvent(delivery.Id, orderId, clientId));
        return delivery;
    }

    public void Assign(Guid courierId)
    {
        if (Status != DeliveryStatus.Pending)
            throw new InvalidDeliveryStatusTransitionException(Status, DeliveryStatus.Assigned);

        CourierId = courierId;
        Status = DeliveryStatus.Assigned;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new DeliveryAssignedDomainEvent(Id, OrderId, courierId));
    }

    public void PickUp()
    {
        if (Status != DeliveryStatus.Assigned)
            throw new InvalidDeliveryStatusTransitionException(Status, DeliveryStatus.InProgress);

        Status = DeliveryStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new DeliveryPickedUpDomainEvent(Id, OrderId, CourierId!.Value));
    }

    public void Complete()
    {
        if (Status != DeliveryStatus.InProgress)
            throw new InvalidDeliveryStatusTransitionException(Status, DeliveryStatus.Delivered);

        Status = DeliveryStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new DeliveryCompletedDomainEvent(Id, OrderId, CorrelationId, CourierId!.Value));
    }

    public void Fail(string reason)
    {
        if (Status == DeliveryStatus.Delivered)
            throw new InvalidDeliveryStatusTransitionException(Status, DeliveryStatus.Failed);

        Status = DeliveryStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new DeliveryFailedDomainEvent(Id, OrderId, reason));
    }
}
