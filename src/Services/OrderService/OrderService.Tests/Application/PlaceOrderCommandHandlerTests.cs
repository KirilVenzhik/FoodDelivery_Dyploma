using Contracts.Events;
using FluentAssertions;
using Moq;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Exceptions;
using Xunit;

namespace OrderService.Tests.Application;

public class PlaceOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly Mock<IRestaurantServiceClient> _restaurantClientMock = new();
    private readonly Mock<IOutboxService> _outboxServiceMock = new();
    private readonly PlaceOrderCommandHandler _handler;

    private static readonly Guid MenuItemId = Guid.NewGuid();

    public PlaceOrderCommandHandlerTests()
    {
        _handler = new PlaceOrderCommandHandler(
            _repositoryMock.Object,
            _restaurantClientMock.Object,
            _outboxServiceMock.Object);

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _outboxServiceMock
            .Setup(x => x.SaveMessageAsync(It.IsAny<PaymentRequestedEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static RestaurantDto OpenRestaurantWith(Guid menuItemId, bool isAvailable = true) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Restaurant",
        Status = "Open",
        MenuItems =
        [
            new MenuItemDto { Id = menuItemId, Name = "Pizza", Price = 12.00m, IsAvailable = isAvailable }
        ]
    };

    [Fact]
    public async Task Handle_ValidRequest_CreatesOrderAndSavesToOutbox()
    {
        _restaurantClientMock
            .Setup(x => x.GetRestaurantWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenRestaurantWith(MenuItemId));

        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [new OrderItemRequest { MenuItemId = MenuItemId, Quantity = 2 }]);

        var result = await _handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.Status.Should().Be("PendingPayment");
        result.TotalAmount.Should().Be(24.00m);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _outboxServiceMock.Verify(x => x.SaveMessageAsync(
            It.IsAny<PaymentRequestedEvent>(), "payment.requested", It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RestaurantNotFound_ThrowsRestaurantValidationException()
    {
        _restaurantClientMock
            .Setup(x => x.GetRestaurantWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantDto?)null);

        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [new OrderItemRequest { MenuItemId = MenuItemId, Quantity = 1 }]);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<RestaurantValidationException>();
    }

    [Fact]
    public async Task Handle_MenuItemUnavailable_ThrowsRestaurantValidationException()
    {
        _restaurantClientMock
            .Setup(x => x.GetRestaurantWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenRestaurantWith(MenuItemId, isAvailable: false));

        var command = new PlaceOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [new OrderItemRequest { MenuItemId = MenuItemId, Quantity = 1 }]);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<RestaurantValidationException>()
            .WithMessage("*not currently available*");
    }

    [Fact]
    public async Task Handle_EmptyItems_ThrowsOrderItemsEmptyException()
    {
        _restaurantClientMock
            .Setup(x => x.GetRestaurantWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenRestaurantWith(MenuItemId));

        var command = new PlaceOrderCommand(Guid.NewGuid(), Guid.NewGuid(), []);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<OrderItemsEmptyException>();
    }
}
