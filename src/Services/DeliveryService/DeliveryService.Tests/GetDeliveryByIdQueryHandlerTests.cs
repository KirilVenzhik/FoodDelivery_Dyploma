using FluentAssertions;
using Moq;
using DeliveryService.Application.Interfaces;
using DeliveryService.Application.Queries;
using DeliveryService.Domain.Entities;
using DeliveryService.Domain.Exceptions;

namespace DeliveryService.Tests;

public class GetDeliveryByIdQueryHandlerTests
{
    private readonly Mock<IDeliveryRepository> _mockRepo = new();
    private readonly GetDeliveryByIdQueryHandler _handler;

    public GetDeliveryByIdQueryHandlerTests()
    {
        _handler = new GetDeliveryByIdQueryHandler(_mockRepo.Object);
    }

    [Fact]
    public async Task Handle_ExistingDelivery_ReturnsDeliveryResponse()
    {
        var delivery = Delivery.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _mockRepo.Setup(r => r.GetByIdAsync(delivery.Id, default)).ReturnsAsync(delivery);

        var result = await _handler.HandleAsync(
            new GetDeliveryByIdQuery(delivery.Id, delivery.ClientId, "Admin"));

        result.Id.Should().Be(delivery.Id);
        result.OrderId.Should().Be(delivery.OrderId);
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_UnknownId_ThrowsDeliveryNotFoundException()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Delivery?)null);

        var act = async () => await _handler.HandleAsync(
            new GetDeliveryByIdQuery(id, Guid.NewGuid(), "Admin"));

        await act.Should().ThrowAsync<DeliveryNotFoundException>();
    }

    [Fact]
    public async Task Handle_CustomerAccessingOthersDelivery_ThrowsUnauthorizedAccessException()
    {
        var delivery = Delivery.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _mockRepo.Setup(r => r.GetByIdAsync(delivery.Id, default)).ReturnsAsync(delivery);

        var act = async () => await _handler.HandleAsync(
            new GetDeliveryByIdQuery(delivery.Id, Guid.NewGuid(), "Customer"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
