using FluentAssertions;
using Moq;
using OrderService.Application.Commands;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;

namespace OrderService.Tests.Application;

public class ChangeOrderStatusCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly ChangeOrderStatusCommandHandler _handler;

    public ChangeOrderStatusCommandHandlerTests()
        => _handler = new ChangeOrderStatusCommandHandler(_repo.Object);

    private static Order MakePaidOrder()
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Item", 5m, 1);
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), [item], Guid.NewGuid());
        order.StartPayment();
        order.CompletePayment();
        return order;
    }

    [Fact]
    public async Task Handle_PaidToInPreparation_UpdatesStatusAndReturnsResponse()
    {
        var order = MakePaidOrder();
        _repo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.HandleAsync(new ChangeOrderStatusCommand(order.Id, "InPreparation"));

        result.Status.Should().Be(OrderStatus.InPreparation.ToString());
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsOrderNotFoundException()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var act = () => _handler.HandleAsync(new ChangeOrderStatusCommand(id, "InPreparation"));

        await act.Should().ThrowAsync<OrderNotFoundException>();
    }

    [Fact]
    public async Task Handle_InvalidTransition_ThrowsInvalidOrderStatusTransitionException()
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Item", 5m, 1);
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), [item], Guid.NewGuid());
        _repo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var act = () => _handler.HandleAsync(
            new ChangeOrderStatusCommand(order.Id, "InPreparation"));

        await act.Should().ThrowAsync<InvalidOrderStatusTransitionException>();
    }
}
