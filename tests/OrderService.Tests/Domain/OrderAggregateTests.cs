using FluentAssertions;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Events;
using OrderService.Domain.Exceptions;

namespace OrderService.Tests.Domain;

public class OrderAggregateTests
{
    private static Order MakeValidOrder()
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Burger", 10m, 2);
        return Order.Create(Guid.NewGuid(), Guid.NewGuid(), [item], Guid.NewGuid());
    }

    [Fact]
    public void Create_WithValidItems_SetsStatusPendingPayment()
    {
        var order = MakeValidOrder();

        order.Status.Should().Be(OrderStatus.PendingPayment);
        order.Items.Should().HaveCount(1);
        order.TotalAmount.Should().Be(20m);
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
        var order = MakeValidOrder();

        order.DomainEvents.Should().ContainSingle(e => e is OrderCreatedDomainEvent);
        var evt = (OrderCreatedDomainEvent)order.DomainEvents.First();
        evt.OrderId.Should().Be(order.Id);
        evt.TotalAmount.Should().Be(20m);
    }

    [Fact]
    public void CompletePayment_SetsStatusPaid()
    {
        var order = MakeValidOrder();
        order.StartPayment();

        order.CompletePayment();

        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void CompletePayment_RaisesOrderPaidDomainEvent()
    {
        var order = MakeValidOrder();
        order.StartPayment();
        order.ClearDomainEvents();

        order.CompletePayment();

        order.DomainEvents.Should().ContainSingle(e => e is OrderPaidDomainEvent);
    }

    [Fact]
    public void FailPayment_SetsStatusPaymentFailed()
    {
        var order = MakeValidOrder();
        order.StartPayment();

        order.FailPayment("Card declined");

        order.Status.Should().Be(OrderStatus.PaymentFailed);
    }

    [Fact]
    public void FailPayment_RaisesOrderPaymentFailedDomainEvent()
    {
        var order = MakeValidOrder();
        order.StartPayment();
        order.ClearDomainEvents();

        order.FailPayment("Card declined");

        order.DomainEvents.Should().ContainSingle(e => e is OrderPaymentFailedDomainEvent);
        var evt = (OrderPaymentFailedDomainEvent)order.DomainEvents.First();
        evt.Reason.Should().Be("Card declined");
    }

    [Fact]
    public void ChangeStatus_ValidTransition_UpdatesStatus()
    {
        var order = MakeValidOrder();
        order.StartPayment();
        order.CompletePayment();

        order.ChangeStatus(OrderStatus.InPreparation);

        order.Status.Should().Be(OrderStatus.InPreparation);
    }

    [Fact]
    public void ChangeStatus_InvalidTransition_ThrowsInvalidOrderStatusTransitionException()
    {
        var order = MakeValidOrder();

        var act = () => order.ChangeStatus(OrderStatus.Delivered);

        act.Should().Throw<InvalidOrderStatusTransitionException>();
    }
}
