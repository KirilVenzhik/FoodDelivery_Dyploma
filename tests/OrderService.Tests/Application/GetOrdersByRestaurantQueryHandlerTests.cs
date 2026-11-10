using FluentAssertions;
using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Queries;
using OrderService.Domain.Entities;

namespace OrderService.Tests.Application;

public class GetOrdersByRestaurantQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly Mock<IRestaurantServiceClient> _restaurantClient = new();
    private readonly GetOrdersByRestaurantQueryHandler _handler;

    public GetOrdersByRestaurantQueryHandlerTests()
    {
        _handler = new GetOrdersByRestaurantQueryHandler(_repo.Object, _restaurantClient.Object);
    }

    private Guid SetupRestaurant(Guid ownerId)
    {
        var restaurantId = Guid.NewGuid();
        var item = OrderItem.Create(Guid.NewGuid(), "Burger", 10m, 1);
        var order = Order.Create(Guid.NewGuid(), restaurantId, [item], Guid.NewGuid());

        _restaurantClient.Setup(c => c.GetRestaurantWithItemsAsync(restaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RestaurantDto { Id = restaurantId, OwnerId = ownerId, Name = "Test Bistro", Status = "Open" });
        _repo.Setup(r => r.GetByRestaurantIdAsync(restaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([order]);

        return restaurantId;
    }

    [Fact]
    public async Task Handle_OwnerOfRestaurant_ReturnsOrders()
    {
        var ownerId = Guid.NewGuid();
        var restaurantId = SetupRestaurant(ownerId);

        var result = await _handler.HandleAsync(new GetOrdersByRestaurantQuery(restaurantId, ownerId));

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_OwnerOfAnotherRestaurant_ThrowsUnauthorizedAccessException()
    {
        var restaurantId = SetupRestaurant(Guid.NewGuid());

        var act = () => _handler.HandleAsync(new GetOrdersByRestaurantQuery(restaurantId, Guid.NewGuid()));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
