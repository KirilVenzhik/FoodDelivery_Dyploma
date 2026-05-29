using FluentAssertions;
using Moq;
using OrderService.Application.Commands;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using Xunit;

namespace OrderService.Tests.Application;

public class ChangeOrderStatusCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly ChangeOrderStatusCommandHandler _handler;

    public ChangeOrderStatusCommandHandlerTests()
    {
        _handler = new ChangeOrderStatusCommandHandler(_repositoryMock.Object);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static Order CreateOrderInStatus(OrderStatus target)
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Pizza", 10.00m, 1);
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), [item], Guid.NewGuid());

        switch (target)
        {
            case OrderStatus.Paid:
                order.CompletePayment();
                break;
            case OrderStatus.InPreparation:
                order.CompletePayment();
                order.ChangeStatus(OrderStatus.InPreparation);
                break;
        }

        return order;
    }

    [Fact]
    public async Task Handle_ValidTransition_UpdatesStatus()
    {
        var order = CreateOrderInStatus(OrderStatus.Paid);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.HandleAsync(new ChangeOrderStatusCommand(order.Id, "InPreparation"));

        result.Status.Should().Be("InPreparation");
        order.Status.Should().Be(OrderStatus.InPreparation);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsOrderNotFoundException()
    {
        var orderId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var act = async () => await _handler.HandleAsync(new ChangeOrderStatusCommand(orderId, "InPreparation"));

        await act.Should().ThrowAsync<OrderNotFoundException>();
    }

    [Fact]
    public async Task Handle_InvalidTransition_ThrowsInvalidOrderStatusTransitionException()
    {
        var order = CreateOrderInStatus(OrderStatus.PendingPayment); // not in AllowedTransitions table

        _repositoryMock
            .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var act = async () => await _handler.HandleAsync(new ChangeOrderStatusCommand(order.Id, "InPreparation"));

        await act.Should().ThrowAsync<InvalidOrderStatusTransitionException>();
    }
}
