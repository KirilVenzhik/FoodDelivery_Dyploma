using Contracts.Events;
using DeliveryService.Application.Commands;
using DeliveryService.Application.Interfaces;
using DeliveryService.Domain.Entities;
using DeliveryService.Domain.Exceptions;
using EventBus.Abstractions;
using FluentAssertions;
using Moq;

namespace DeliveryService.Tests.Application;

public class CompleteDeliveryCommandHandlerTests
{
    private readonly Mock<IDeliveryRepository> _repo = new();
    private readonly Mock<IEventBus> _eventBus = new();
    private readonly CompleteDeliveryCommandHandler _handler;

    public CompleteDeliveryCommandHandlerTests()
        => _handler = new CompleteDeliveryCommandHandler(_repo.Object, _eventBus.Object);

    private static Delivery MakeInProgressDelivery(Guid courierId)
    {
        var d = Delivery.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        d.Assign(courierId);
        d.PickUp();
        return d;
    }

    [Fact]
    public async Task Handle_InProgress_PublishesDeliveryCompletedEventWithCorrectFields()
    {
        var courierId = Guid.NewGuid();
        var delivery = MakeInProgressDelivery(courierId);
        _repo.Setup(r => r.GetByIdAsync(delivery.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);

        await _handler.HandleAsync(new CompleteDeliveryCommand(delivery.Id, courierId));

        _eventBus.Verify(e => e.PublishAsync(
            It.Is<DeliveryCompletedEvent>(evt =>
                evt.OrderId == delivery.OrderId && evt.CourierId == courierId),
            "delivery.completed",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WrongCourier_ThrowsUnauthorizedAccessException()
    {
        var courierId = Guid.NewGuid();
        var delivery = MakeInProgressDelivery(courierId);
        _repo.Setup(r => r.GetByIdAsync(delivery.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);

        var act = () => _handler.HandleAsync(new CompleteDeliveryCommand(delivery.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsDeliveryNotFoundException()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Delivery?)null);

        var act = () => _handler.HandleAsync(new CompleteDeliveryCommand(Guid.NewGuid(), Guid.NewGuid()));

        await act.Should().ThrowAsync<DeliveryNotFoundException>();
    }
}
