using FluentAssertions;
using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Queries;
using OrderService.Domain.Entities;

namespace OrderService.Tests.Application;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly Mock<IRestaurantServiceClient> _restaurantClient = new();
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _handler = new GetOrderByIdQueryHandler(_repo.Object, _restaurantClient.Object);
    }

    private Order SetupOrder(Guid clientId, Guid restaurantOwnerId)
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Burger", 10m, 1);
        var order = Order.Create(clientId, Guid.NewGuid(), [item], Guid.NewGuid());

        _repo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _restaurantClient.Setup(c => c.GetRestaurantWithItemsAsync(order.RestaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestaurantDto { Id = order.RestaurantId, OwnerId = restaurantOwnerId, Name = "Test Bistro", Status = "Open" });

        return order;
    }

    [Fact]
    public async Task Handle_CustomerReadsOwnOrder_ReturnsOrder()
    {
        var clientId = Guid.NewGuid();
        var order = SetupOrder(clientId, Guid.NewGuid());

        var result = await _handler.HandleAsync(new GetOrderByIdQuery(order.Id, clientId));

        result.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task Handle_CustomerReadsSomeoneElsesOrder_ThrowsUnauthorizedAccessException()
    {
        var order = SetupOrder(Guid.NewGuid(), Guid.NewGuid());

        var act = () => _handler.HandleAsync(new GetOrderByIdQuery(order.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_OwnerOfOrderRestaurant_ReturnsOrder()
    {
        var ownerId = Guid.NewGuid();
        var order = SetupOrder(Guid.NewGuid(), ownerId);

        var result = await _handler.HandleAsync(new GetOrderByIdQuery(order.Id, ownerId));

        result.Id.Should().Be(order.Id);
    }
}
