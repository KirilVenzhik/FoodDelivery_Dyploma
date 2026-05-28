using Xunit;
using FluentAssertions;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;

namespace OrderService.Tests.Domain;

public class OrderAggregateTests
{
    private static Order CreatePaidOrder()
    {
        var items = new[] { OrderItem.Create(Guid.NewGuid(), "Pizza", 10m, 2) };
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), items);
        order.MarkPaymentCompleted();
        return order;
    }

    [Fact]
    public void Create_SetsStatusToPendingPayment()
    {
        var items = new[] { OrderItem.Create(Guid.NewGuid(), "Item", 5m, 1) };
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), items);
        order.Status.Should().Be(OrderStatus.PendingPayment);
    }

    [Fact]
    public void Create_CalculatesTotalAmountFromItems()
    {
        var items = new[]
        {
            OrderItem.Create(Guid.NewGuid(), "A", 10m, 2),
            OrderItem.Create(Guid.NewGuid(), "B", 5m, 3)
        };
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), items);
        order.TotalAmount.Should().Be(35m);
    }

    [Fact]
    public void Create_SetsClientAndRestaurantIds()
    {
        var clientId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var items = new[] { OrderItem.Create(Guid.NewGuid(), "Item", 5m, 1) };
        var order = Order.Create(clientId, restaurantId, items);
        order.ClientId.Should().Be(clientId);
        order.RestaurantId.Should().Be(restaurantId);
    }

    [Fact]
    public void RequestPayment_SetsPaymentRequestedToTrue()
    {
        var items = new[] { OrderItem.Create(Guid.NewGuid(), "Item", 5m, 1) };
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), items);
        order.RequestPayment();
        order.PaymentRequested.Should().BeTrue();
    }

    [Fact]
    public void MarkPaymentCompleted_SetsStatusToPaid()
    {
        var items = new[] { OrderItem.Create(Guid.NewGuid(), "Item", 5m, 1) };
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), items);
        order.MarkPaymentCompleted();
        order.Status.Should().Be(OrderStatus.Paid);
        order.PaymentCompleted.Should().BeTrue();
    }

    [Fact]
    public void MarkPaymentFailed_SetsStatusToPaymentFailed()
    {
        var items = new[] { OrderItem.Create(Guid.NewGuid(), "Item", 5m, 1) };
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), items);
        order.MarkPaymentFailed();
        order.Status.Should().Be(OrderStatus.PaymentFailed);
        order.PaymentFailed.Should().BeTrue();
    }

    [Fact]
    public void StartPreparation_WhenStatusIsPaid_SetsInPreparation()
    {
        var order = CreatePaidOrder();
        order.StartPreparation();
        order.Status.Should().Be(OrderStatus.InPreparation);
    }

    [Fact]
    public void StartPreparation_WhenStatusIsPendingPayment_ThrowsOrderDomainException()
    {
        var items = new[] { OrderItem.Create(Guid.NewGuid(), "Item", 5m, 1) };
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), items);

        var act = () => order.StartPreparation();

        act.Should().Throw<OrderDomainException>()
            .WithMessage("*PendingPayment*");
    }

    [Fact]
    public void StartPreparation_WhenStatusIsPaymentFailed_ThrowsOrderDomainException()
    {
        var items = new[] { OrderItem.Create(Guid.NewGuid(), "Item", 5m, 1) };
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), items);
        order.MarkPaymentFailed();

        var act = () => order.StartPreparation();

        act.Should().Throw<OrderDomainException>();
    }

    [Fact]
    public void MarkReadyForDelivery_WhenStatusIsInPreparation_SetsStatus()
    {
        var order = CreatePaidOrder();
        order.StartPreparation();
        order.MarkReadyForDelivery();
        order.Status.Should().Be(OrderStatus.ReadyForDelivery);
    }

    [Fact]
    public void MarkReadyForDelivery_WhenStatusIsPaid_ThrowsOrderDomainException()
    {
        var order = CreatePaidOrder();

        var act = () => order.MarkReadyForDelivery();

        act.Should().Throw<OrderDomainException>()
            .WithMessage("*Paid*");
    }

    [Fact]
    public void MarkDelivered_SetsStatusToDelivered()
    {
        var items = new[] { OrderItem.Create(Guid.NewGuid(), "Item", 5m, 1) };
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), items);
        order.MarkDelivered();
        order.Status.Should().Be(OrderStatus.Delivered);
    }
}
