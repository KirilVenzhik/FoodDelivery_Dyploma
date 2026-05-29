using FluentAssertions;
using Moq;
using DeliveryService.Application.Commands;
using DeliveryService.Application.Interfaces;
using DeliveryService.Domain.Entities;
using DeliveryService.Domain.Exceptions;

namespace DeliveryService.Tests;

public class AssignDeliveryCommandHandlerTests
{
    private readonly Mock<IDeliveryRepository> _mockRepo = new();
    private readonly AssignDeliveryCommandHandler _handler;

    public AssignDeliveryCommandHandlerTests()
    {
        _handler = new AssignDeliveryCommandHandler(_mockRepo.Object);
    }

    [Fact]
    public async Task Handle_PendingDelivery_AssignsCorrectly()
    {
        var courierId = Guid.NewGuid();
        var delivery = Delivery.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _mockRepo.Setup(r => r.GetByIdAsync(delivery.Id, default)).ReturnsAsync(delivery);
        _mockRepo.Setup(r => r.SaveChangesAsync(default)).Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(new AssignDeliveryCommand(delivery.Id, courierId));

        result.Status.Should().Be("Assigned");
        result.CourierId.Should().Be(courierId);
    }

    [Fact]
    public async Task Handle_DeliveryNotFound_ThrowsDeliveryNotFoundException()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Delivery?)null);

        var act = async () => await _handler.HandleAsync(new AssignDeliveryCommand(id, Guid.NewGuid()));

        await act.Should().ThrowAsync<DeliveryNotFoundException>();
    }

    [Fact]
    public async Task Handle_AlreadyAssigned_ThrowsDeliveryAlreadyAssignedException()
    {
        var delivery = Delivery.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        delivery.Assign(Guid.NewGuid());

        _mockRepo.Setup(r => r.GetByIdAsync(delivery.Id, default)).ReturnsAsync(delivery);

        var act = async () => await _handler.HandleAsync(new AssignDeliveryCommand(delivery.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<DeliveryAlreadyAssignedException>();
    }
}
