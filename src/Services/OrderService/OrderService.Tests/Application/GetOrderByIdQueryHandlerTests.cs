using FluentAssertions;
using Moq;
using OrderService.Application.Interfaces;
using OrderService.Application.Queries;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;
using Xunit;

namespace OrderService.Tests.Application;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _handler = new GetOrderByIdQueryHandler(_repositoryMock.Object);
    }

    private static Order CreateOrder(Guid clientId)
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Pizza", 10.00m, 1);
        return Order.Create(clientId, Guid.NewGuid(), [item], Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_ExistingOrder_ReturnsOrderResponse()
    {
        var clientId = Guid.NewGuid();
        var order = CreateOrder(clientId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.HandleAsync(new GetOrderByIdQuery(order.Id, clientId, "Customer"));

        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
        result.ClientId.Should().Be(clientId);
        result.Status.Should().Be("PendingPayment");
    }

    [Fact]
    public async Task Handle_UnknownId_ThrowsOrderNotFoundException()
    {
        var orderId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var act = async () => await _handler.HandleAsync(
            new GetOrderByIdQuery(orderId, Guid.NewGuid(), "Customer"));

        await act.Should().ThrowAsync<OrderNotFoundException>();
    }

    [Fact]
    public async Task Handle_CustomerAccessingOthersOrder_ThrowsUnauthorizedAccessException()
    {
        var order = CreateOrder(Guid.NewGuid()); // order belongs to a different client

        _repositoryMock
            .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var act = async () => await _handler.HandleAsync(
            new GetOrderByIdQuery(order.Id, Guid.NewGuid(), "Customer"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
