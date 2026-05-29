using FluentAssertions;
using Moq;
using Contracts.Events;
using Contracts.Abstractions;
using DeliveryService.Application.Commands;
using DeliveryService.Application.Interfaces;
using DeliveryService.Domain.Entities;
using DeliveryService.Domain.Exceptions;
using EventBus.Abstractions;

namespace DeliveryService.Tests;

public class CompleteDeliveryCommandHandlerTests
{
    private readonly Mock<IDeliveryRepository> _mockRepo = new();
    private readonly Mock<IEventBus> _mockEventBus = new();
    private readonly CompleteDeliveryCommandHandler _handler;

    public CompleteDeliveryCommandHandlerTests()
    {
        _handler = new CompleteDeliveryCommandHandler(_mockRepo.Object, _mockEventBus.Object);
    }

    private static Delivery CreateInProgressDelivery(Guid courierId)
    {
        var delivery = Delivery.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        delivery.Assign(courierId);
        delivery.PickUp();
        return delivery;
    }

    [Fact]
    public async Task Handle_ValidCompletion_PublishesDeliveryCompletedEvent()
    {
        var courierId = Guid.NewGuid();
        var delivery = CreateInProgressDelivery(courierId);

        _mockRepo.Setup(r => r.GetByIdAsync(delivery.Id, default)).ReturnsAsync(delivery);
        _mockRepo.Setup(r => r.SaveChangesAsync(default)).Returns(Task.CompletedTask);
        _mockEventBus
            .Setup(b => b.PublishAsync(It.IsAny<DeliveryCompletedEvent>(), "delivery.completed", default))
            .Returns(Task.CompletedTask);

        await _handler.HandleAsync(new CompleteDeliveryCommand(delivery.Id, courierId));

        _mockEventBus.Verify(b => b.PublishAsync(
            It.Is<DeliveryCompletedEvent>(e => e.OrderId == delivery.OrderId && e.CourierId == courierId),
            "delivery.completed",
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_WrongCourier_ThrowsUnauthorizedAccessException()
    {
        var courierId = Guid.NewGuid();
        var delivery = CreateInProgressDelivery(courierId);

        _mockRepo.Setup(r => r.GetByIdAsync(delivery.Id, default)).ReturnsAsync(delivery);

        var act = async () => await _handler.HandleAsync(new CompleteDeliveryCommand(delivery.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_DeliveryNotFound_ThrowsDeliveryNotFoundException()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Delivery?)null);

        var act = async () => await _handler.HandleAsync(new CompleteDeliveryCommand(id, Guid.NewGuid()));

        await act.Should().ThrowAsync<DeliveryNotFoundException>();
    }
}
