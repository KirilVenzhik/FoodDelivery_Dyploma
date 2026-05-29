using FluentAssertions;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Events;
using OrderService.Domain.Exceptions;
using Xunit;

namespace OrderService.Tests.Domain;

public class OrderAggregateTests
{
    private static Order CreateOrder(Guid? clientId = null, Guid? restaurantId = null)
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Pizza", 10.00m, 2);
        return Order.Create(
            clientId ?? Guid.NewGuid(),
            restaurantId ?? Guid.NewGuid(),
            [item],
            Guid.NewGuid());
    }

    private static Order CreateOrderInPaidStatus()
    {
        var order = CreateOrder();
        order.CompletePayment();
        return order;
    }

    [Fact]
    public void Create_WithValidItems_SetsStatusPendingPayment()
    {
        var order = CreateOrder();

        order.Status.Should().Be(OrderStatus.PendingPayment);
        order.TotalAmount.Should().Be(20.00m);
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithNoItems_ThrowsOrderItemsEmptyException()
    {
        var act = () => Order.Create(Guid.NewGuid(), Guid.NewGuid(), [], Guid.NewGuid());

        act.Should().Throw<OrderItemsEmptyException>();
    }

    [Fact]
    public void Create_RaisesOrderCreatedDomainEvent()
    {
        var order = CreateOrder();

        var evt = order.DomainEvents.OfType<OrderCreatedDomainEvent>().Single();
        evt.OrderId.Should().Be(order.Id);
        evt.ClientId.Should().Be(order.ClientId);
        evt.RestaurantId.Should().Be(order.RestaurantId);
        evt.TotalAmount.Should().Be(order.TotalAmount);
    }

    [Fact]
    public void StartPayment_SetsPaymentRequestedTrue()
    {
        var order = CreateOrder();

        order.StartPayment();

        order.PaymentRequested.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.PendingPayment);
    }

    [Fact]
    public void CompletePayment_SetsStatusPaid()
    {
        var order = CreateOrder();

        order.CompletePayment();

        order.Status.Should().Be(OrderStatus.Paid);
        order.PaymentCompleted.Should().BeTrue();
    }

    [Fact]
    public void CompletePayment_RaisesOrderPaidDomainEvent()
    {
        var order = CreateOrder();
        order.ClearDomainEvents();

        order.CompletePayment();

        var evt = order.DomainEvents.OfType<OrderPaidDomainEvent>().Single();
        evt.OrderId.Should().Be(order.Id);
        evt.ClientId.Should().Be(order.ClientId);
    }

    [Fact]
    public void FailPayment_SetsStatusPaymentFailed()
    {
        var order = CreateOrder();

        order.FailPayment("Insufficient funds");

        order.Status.Should().Be(OrderStatus.PaymentFailed);
        order.PaymentFailed.Should().BeTrue();
    }

    [Fact]
    public void FailPayment_RaisesOrderPaymentFailedDomainEvent()
    {
        var order = CreateOrder();
        order.ClearDomainEvents();

        order.FailPayment("Insufficient funds");

        var evt = order.DomainEvents.OfType<OrderPaymentFailedDomainEvent>().Single();
        evt.OrderId.Should().Be(order.Id);
        evt.ClientId.Should().Be(order.ClientId);
        evt.Reason.Should().Be("Insufficient funds");
    }

    [Fact]
    public void ChangeStatus_ValidTransition_UpdatesStatus()
    {
        var order = CreateOrderInPaidStatus();

        order.ChangeStatus(OrderStatus.InPreparation);

        order.Status.Should().Be(OrderStatus.InPreparation);
    }

    [Fact]
    public void ChangeStatus_InvalidTransition_ThrowsInvalidOrderStatusTransitionException()
    {
        var order = CreateOrder(); // PendingPayment — not in transition table

        var act = () => order.ChangeStatus(OrderStatus.InPreparation);

        act.Should().Throw<InvalidOrderStatusTransitionException>();
    }

    [Fact]
    public void ChangeStatus_RaisesOrderStatusChangedDomainEvent()
    {
        var order = CreateOrderInPaidStatus();
        order.ClearDomainEvents();

        order.ChangeStatus(OrderStatus.InPreparation);

        var evt = order.DomainEvents.OfType<OrderStatusChangedDomainEvent>().Single();
        evt.OrderId.Should().Be(order.Id);
        evt.OldStatus.Should().Be(OrderStatus.Paid);
        evt.NewStatus.Should().Be(OrderStatus.InPreparation);
    }
}
