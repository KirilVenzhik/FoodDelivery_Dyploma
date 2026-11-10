using FluentAssertions;
using Moq;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Exceptions;

namespace OrderService.Tests.Application;

public class PlaceOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly Mock<IRestaurantServiceClient> _restaurantClient = new();
    private readonly Mock<IOutboxService> _outbox = new();
    private readonly PlaceOrderCommandHandler _handler;

    public PlaceOrderCommandHandlerTests()
    {
        _handler = new PlaceOrderCommandHandler(_repo.Object, _restaurantClient.Object, _outbox.Object);
    }

    private static RestaurantDto MakeRestaurant(bool active = true, bool itemAvailable = true)
    {
        var itemId = Guid.NewGuid();
        return new RestaurantDto
        {
            Id = Guid.NewGuid(),
            Name = "Test Bistro",
            Status = active ? "Open" : "Closed",
            MenuItems =
            [
                new MenuItemDto { Id = itemId, Name = "Burger", Price = 10m, Available = itemAvailable }
            ]
        };
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesOrderAndSavesToOutbox()
    {
        var restaurant = MakeRestaurant();
        var itemId = restaurant.MenuItems[0].Id;

        _restaurantClient.Setup(c => c.GetRestaurantWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            restaurant.Id,
            [new OrderItemRequest { MenuItemId = itemId, Quantity = 2 }]);

        var result = await _handler.HandleAsync(command);

        _repo.Verify(r => r.AddAsync(It.IsAny<OrderService.Domain.Entities.Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _outbox.Verify(o => o.SaveMessageAsync(It.IsAny<Contracts.Events.PaymentRequestedEvent>(), "payment.requested", It.IsAny<CancellationToken>()), Times.Once);
        result.TotalAmount.Should().Be(20m);
    }

    [Fact]
    public async Task Handle_RestaurantNotFound_ThrowsRestaurantValidationException()
    {
        _restaurantClient.Setup(c => c.GetRestaurantWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantDto?)null);

        var act = () => _handler.HandleAsync(
            new PlaceOrderCommand(Guid.NewGuid(), Guid.NewGuid(),
                [new OrderItemRequest { MenuItemId = Guid.NewGuid(), Quantity = 1 }]));

        await act.Should().ThrowAsync<RestaurantValidationException>();
    }

    [Fact]
    public async Task Handle_MenuItemUnavailable_ThrowsRestaurantValidationException()
    {
        var restaurant = MakeRestaurant(active: true, itemAvailable: false);
        var itemId = restaurant.MenuItems[0].Id;

        _restaurantClient.Setup(c => c.GetRestaurantWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var act = () => _handler.HandleAsync(
            new PlaceOrderCommand(Guid.NewGuid(), restaurant.Id,
                [new OrderItemRequest { MenuItemId = itemId, Quantity = 1 }]));

        await act.Should().ThrowAsync<RestaurantValidationException>();
    }

    [Fact]
    public async Task Handle_EmptyItems_FailsValidation()
    {
        var restaurant = MakeRestaurant();
        _restaurantClient.Setup(c => c.GetRestaurantWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var act = () => _handler.HandleAsync(
            new PlaceOrderCommand(Guid.NewGuid(), restaurant.Id, []));

        await act.Should().ThrowAsync<OrderItemsEmptyException>();
    }
}
