using OrderService.Domain.Enums;
using OrderService.Domain.Events;
using OrderService.Domain.Exceptions;
using Shared.Primitives;

namespace OrderService.Domain.Entities;

public class Order : Entity
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.Paid]             = [OrderStatus.InPreparation],
        [OrderStatus.InPreparation]    = [OrderStatus.ReadyForDelivery],
        [OrderStatus.ReadyForDelivery] = [OrderStatus.InDelivery],
        [OrderStatus.InDelivery]       = [OrderStatus.Delivered],
    };

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
        IEnumerable<OrderItem> items,
        Guid correlationId)
    {
        var itemList = items.ToList();

        if (itemList.Count == 0)
            throw new OrderItemsEmptyException();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CorrelationId = correlationId,
            ClientId = clientId,
            RestaurantId = restaurantId,
            Status = OrderStatus.PendingPayment,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in itemList)
            order._items.Add(item);

        order.TotalAmount = order._items.Sum(i => i.LineTotal);

        order.RaiseDomainEvent(new OrderCreatedDomainEvent
        {
            OrderId = order.Id,
            ClientId = order.ClientId,
            RestaurantId = order.RestaurantId,
            TotalAmount = order.TotalAmount
        });

        return order;
    }

    public void StartPayment()
    {
        if (Status != OrderStatus.PendingPayment)
            throw new InvalidOrderStatusTransitionException(
                $"Cannot start payment: order is in '{Status}' status, expected 'PendingPayment'.");

        PaymentRequested = true;
    }

    public void CompletePayment()
    {
        Status = OrderStatus.Paid;
        PaymentCompleted = true;

        RaiseDomainEvent(new OrderPaidDomainEvent
        {
            OrderId = Id,
            ClientId = ClientId
        });
    }

    public void FailPayment(string reason)
    {
        Status = OrderStatus.PaymentFailed;
        PaymentFailed = true;

        RaiseDomainEvent(new OrderPaymentFailedDomainEvent
        {
            OrderId = Id,
            ClientId = ClientId,
            Reason = reason
        });
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidOrderStatusTransitionException(Status, newStatus);

        var oldStatus = Status;
        Status = newStatus;

        RaiseDomainEvent(new OrderStatusChangedDomainEvent
        {
            OrderId = Id,
            OldStatus = oldStatus,
            NewStatus = newStatus
        });
    }
}
