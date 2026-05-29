using FluentAssertions;
using Moq;
using OrderService.Application.Interfaces;
using OrderService.Application.Queries;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;

namespace OrderService.Tests.Application;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
        => _handler = new GetOrderByIdQueryHandler(_repo.Object);

    private static Order MakeOrder(Guid clientId)
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Item", 5m, 1);
        return Order.Create(clientId, Guid.NewGuid(), [item], Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_ExistingOrder_ReturnsOrderResponse()
    {
        var clientId = Guid.NewGuid();
        var order = MakeOrder(clientId);
        _repo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.HandleAsync(new GetOrderByIdQuery(order.Id, clientId, "Customer"));

        result.Id.Should().Be(order.Id);
        result.Status.Should().Be(OrderStatus.PendingPayment.ToString());
    }

    [Fact]
    public async Task Handle_UnknownId_ThrowsOrderNotFoundException()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var act = () => _handler.HandleAsync(new GetOrderByIdQuery(id, Guid.NewGuid(), "Customer"));

        await act.Should().ThrowAsync<OrderNotFoundException>();
    }

    [Fact]
    public async Task Handle_DifferentClientId_ThrowsUnauthorizedAccessException()
    {
        var order = MakeOrder(Guid.NewGuid());
        _repo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var act = () => _handler.HandleAsync(
            new GetOrderByIdQuery(order.Id, Guid.NewGuid(), "Customer"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
