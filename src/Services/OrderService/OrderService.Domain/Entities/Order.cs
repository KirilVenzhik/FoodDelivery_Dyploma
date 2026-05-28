using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using Shared.Primitives;

namespace OrderService.Domain.Entities;

public class Order : Entity
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; }

    public Guid CorrelationId { get; private set; }

    public Guid ClientId { get; private set; }

    public Guid RestaurantId { get; private set; }

    public decimal TotalAmount { get; private set; }

    public OrderStatus Status { get; private set; }

    public bool PaymentRequested { get; private set; }

    public bool PaymentCompleted { get; private set; }

    public bool PaymentFailed { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;

    private Order() { }

    public static Order Create(
        Guid clientId,
        Guid restaurantId,
        IEnumerable<OrderItem> items)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            ClientId = clientId,
            RestaurantId = restaurantId,
            Status = OrderStatus.PendingPayment,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in items)
        {
            order._items.Add(item);
        }

        order.TotalAmount = order._items.Sum(i => i.LineTotal);

        return order;
    }

    public void RequestPayment()
    {
        PaymentRequested = true;
    }

    public void MarkPaymentCompleted()
    {
        PaymentCompleted = true;
        Status = OrderStatus.Paid;
    }

    public void MarkPaymentFailed()
    {
        PaymentFailed = true;
        Status = OrderStatus.PaymentFailed;
    }

    public void StartPreparation()
    {
        if (Status != OrderStatus.Paid)
            throw new OrderDomainException($"Cannot start preparation: order is in '{Status}' status.");
        Status = OrderStatus.InPreparation;
    }

    public void MarkReadyForDelivery()
    {
        if (Status != OrderStatus.InPreparation)
            throw new OrderDomainException($"Cannot mark ready for delivery: order is in '{Status}' status.");
        Status = OrderStatus.ReadyForDelivery;
    }

    public void MarkInDelivery()
    {
        Status = OrderStatus.InDelivery;
    }

    public void MarkDelivered()
    {
        Status = OrderStatus.Delivered;
    }
}
