using Xunit;
using FluentAssertions;
using Moq;
using OrderService.Application.Commands;
using OrderService.Application.Common;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Tests.Application;

public class UpdateOrderStatusCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly UpdateOrderStatusCommandHandler _sut;

    public UpdateOrderStatusCommandHandlerTests()
    {
        _repositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _sut = new UpdateOrderStatusCommandHandler(_repositoryMock.Object);
    }

    private static Order CreatePaidOrder()
    {
        var items = new[] { OrderItem.Create(Guid.NewGuid(), "Pizza", 10m, 1) };
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), items);
        order.MarkPaymentCompleted();
        return order;
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ReturnsNotFound()
    {
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _sut.HandleAsync(new UpdateOrderStatusCommand(Guid.NewGuid(), "InPreparation"));

        result.Status.Should().Be(CommandResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WithInPreparation_WhenOrderIsPaid_ReturnsSuccess()
    {
        var order = CreatePaidOrder();
        _repositoryMock.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.HandleAsync(new UpdateOrderStatusCommand(order.Id, "InPreparation"));

        result.Status.Should().Be(CommandResultStatus.Success);
        order.Status.Should().Be(OrderStatus.InPreparation);
    }

    [Fact]
    public async Task Handle_WithReadyForDelivery_WhenOrderIsInPreparation_ReturnsSuccess()
    {
        var order = CreatePaidOrder();
        order.StartPreparation();
        _repositoryMock.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.HandleAsync(new UpdateOrderStatusCommand(order.Id, "ReadyForDelivery"));

        result.Status.Should().Be(CommandResultStatus.Success);
        order.Status.Should().Be(OrderStatus.ReadyForDelivery);
    }

    [Fact]
    public async Task Handle_WithInvalidStatusString_ReturnsUnprocessableEntity()
    {
        var order = CreatePaidOrder();
        _repositoryMock.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.HandleAsync(new UpdateOrderStatusCommand(order.Id, "Delivered"));

        result.Status.Should().Be(CommandResultStatus.UnprocessableEntity);
    }

    [Fact]
    public async Task Handle_WithInPreparation_WhenOrderIsNotPaid_ReturnsUnprocessableEntity()
    {
        var items = new[] { OrderItem.Create(Guid.NewGuid(), "Pizza", 10m, 1) };
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), items);
        _repositoryMock.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.HandleAsync(new UpdateOrderStatusCommand(order.Id, "InPreparation"));

        result.Status.Should().Be(CommandResultStatus.UnprocessableEntity);
        result.Error.Should().Contain("PendingPayment");
    }
}
