using Xunit;
using FluentAssertions;
using Moq;
using OrderService.Application.Commands;
using OrderService.Application.Common;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Shared.Primitives;

namespace OrderService.Tests.Application;

public class PlaceOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly Mock<IRestaurantServiceClient> _restaurantClientMock = new();
    private readonly PlaceOrderCommandHandler _sut;

    private static readonly Guid MenuItemId = Guid.NewGuid();
    private static readonly Guid RestaurantId = Guid.NewGuid();

    private static readonly RestaurantDto OpenRestaurant = new()
    {
        Id = RestaurantId,
        Name = "Test Restaurant",
        Status = "Open",
        MenuItems =
        [
            new MenuItemDto { Id = MenuItemId, Name = "Pizza", Price = 10m, IsAvailable = true }
        ]
    };

    public PlaceOrderCommandHandlerTests()
    {
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<OrderService.Domain.Entities.Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repositoryMock.Setup(x => x.AddOutboxMessageAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new PlaceOrderCommandHandler(_repositoryMock.Object, _restaurantClientMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRestaurantNotFound_ReturnsNotFound()
    {
        _restaurantClientMock.Setup(x => x.GetRestaurantAsync(RestaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantDto?)null);

        var result = await _sut.HandleAsync(
            new PlaceOrderCommand(Guid.NewGuid(), RestaurantId, [new() { MenuItemId = MenuItemId, Quantity = 1 }]));

        result.Status.Should().Be(CommandResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenRestaurantClosed_ReturnsUnprocessableEntity()
    {
        var closedRestaurant = new RestaurantDto { Id = RestaurantId, Status = "Closed", MenuItems = [] };
        _restaurantClientMock.Setup(x => x.GetRestaurantAsync(RestaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(closedRestaurant);

        var result = await _sut.HandleAsync(
            new PlaceOrderCommand(Guid.NewGuid(), RestaurantId, [new() { MenuItemId = MenuItemId, Quantity = 1 }]));

        result.Status.Should().Be(CommandResultStatus.UnprocessableEntity);
        result.Error.Should().Contain("not accepting");
    }

    [Fact]
    public async Task Handle_WhenMenuItemNotFound_ReturnsUnprocessableEntity()
    {
        _restaurantClientMock.Setup(x => x.GetRestaurantAsync(RestaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenRestaurant);

        var unknownItemId = Guid.NewGuid();
        var result = await _sut.HandleAsync(
            new PlaceOrderCommand(Guid.NewGuid(), RestaurantId, [new() { MenuItemId = unknownItemId, Quantity = 1 }]));

        result.Status.Should().Be(CommandResultStatus.UnprocessableEntity);
    }

    [Fact]
    public async Task Handle_WhenMenuItemUnavailable_ReturnsUnprocessableEntity()
    {
        var restaurantWithUnavailableItem = new RestaurantDto
        {
            Id = RestaurantId,
            Status = "Open",
            MenuItems = [new MenuItemDto { Id = MenuItemId, Name = "Pizza", Price = 10m, IsAvailable = false }]
        };
        _restaurantClientMock.Setup(x => x.GetRestaurantAsync(RestaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurantWithUnavailableItem);

        var result = await _sut.HandleAsync(
            new PlaceOrderCommand(Guid.NewGuid(), RestaurantId, [new() { MenuItemId = MenuItemId, Quantity = 1 }]));

        result.Status.Should().Be(CommandResultStatus.UnprocessableEntity);
        result.Error.Should().Contain("not available");
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccess()
    {
        _restaurantClientMock.Setup(x => x.GetRestaurantAsync(RestaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenRestaurant);

        var result = await _sut.HandleAsync(
            new PlaceOrderCommand(Guid.NewGuid(), RestaurantId, [new() { MenuItemId = MenuItemId, Quantity = 2 }]));

        result.Status.Should().Be(CommandResultStatus.Success);
        result.CreatedId.Should().NotBeNull().And.NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithValidRequest_SavesOrderAndOutboxMessage()
    {
        _restaurantClientMock.Setup(x => x.GetRestaurantAsync(RestaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenRestaurant);

        await _sut.HandleAsync(
            new PlaceOrderCommand(Guid.NewGuid(), RestaurantId, [new() { MenuItemId = MenuItemId, Quantity = 1 }]));

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<OrderService.Domain.Entities.Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.AddOutboxMessageAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
