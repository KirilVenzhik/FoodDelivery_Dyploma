using FluentAssertions;
using Moq;
using RestaurantService.Application.Commands;
using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Entities;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Tests;

public class RemoveMenuItemCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repoMock = new();

    private RemoveMenuItemCommandHandler CreateHandler() =>
        new(_repoMock.Object);

    [Fact]
    public async Task Handle_WithValidData_RemovesItem()
    {
        var ownerId = Guid.NewGuid();
        var restaurant = Restaurant.Create(ownerId, "Test", "Desc", "Addr");
        var item = restaurant.AddMenuItem("Burger", "Tasty", 9.99m);
        _repoMock.Setup(r => r.GetByIdWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RemoveMenuItemCommand(restaurant.Id, item.Id, ownerId);
        await CreateHandler().HandleAsync(command);

        restaurant.MenuItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithUnknownItemId_ThrowsMenuItemNotFoundException()
    {
        var ownerId = Guid.NewGuid();
        var restaurant = Restaurant.Create(ownerId, "Test", "Desc", "Addr");
        _repoMock.Setup(r => r.GetByIdWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var command = new RemoveMenuItemCommand(restaurant.Id, Guid.NewGuid(), ownerId);
        var act = async () => await CreateHandler().HandleAsync(command);
        await act.Should().ThrowAsync<MenuItemNotFoundException>();
    }

    [Fact]
    public async Task Handle_WithWrongOwner_ThrowsRestaurantOwnershipException()
    {
        var restaurant = Restaurant.Create(Guid.NewGuid(), "Test", "Desc", "Addr");
        _repoMock.Setup(r => r.GetByIdWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var command = new RemoveMenuItemCommand(restaurant.Id, Guid.NewGuid(), Guid.NewGuid());
        var act = async () => await CreateHandler().HandleAsync(command);
        await act.Should().ThrowAsync<RestaurantOwnershipException>();
    }
}
