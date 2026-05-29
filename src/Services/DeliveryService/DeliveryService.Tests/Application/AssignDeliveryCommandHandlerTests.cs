using DeliveryService.Application.Commands;
using DeliveryService.Application.Interfaces;
using DeliveryService.Domain.Entities;
using DeliveryService.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace DeliveryService.Tests.Application;

public class AssignDeliveryCommandHandlerTests
{
    private readonly Mock<IDeliveryRepository> _repo = new();
    private readonly AssignDeliveryCommandHandler _handler;

    public AssignDeliveryCommandHandlerTests()
        => _handler = new AssignDeliveryCommandHandler(_repo.Object);

    private static Delivery MakePendingDelivery()
        => Delivery.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task Handle_PendingDelivery_AssignsAndReturnsResponse()
    {
        var delivery = MakePendingDelivery();
        var courierId = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(delivery.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);

        var result = await _handler.HandleAsync(new AssignDeliveryCommand(delivery.Id, courierId));

        result.Status.Should().Be("Assigned");
        result.CourierId.Should().Be(courierId);
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsDeliveryNotFoundException()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Delivery?)null);

        var act = () => _handler.HandleAsync(new AssignDeliveryCommand(Guid.NewGuid(), Guid.NewGuid()));

        await act.Should().ThrowAsync<DeliveryNotFoundException>();
    }

    [Fact]
    public async Task Handle_AlreadyAssigned_ThrowsDeliveryAlreadyAssignedException()
    {
        var delivery = MakePendingDelivery();
        delivery.Assign(Guid.NewGuid());
        _repo.Setup(r => r.GetByIdAsync(delivery.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);

        var act = () => _handler.HandleAsync(new AssignDeliveryCommand(delivery.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<DeliveryAlreadyAssignedException>();
    }
}
