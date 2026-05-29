using FluentAssertions;
using Moq;
using RestaurantService.Application.Commands;
using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Entities;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Tests.Application;

public class RemoveMenuItemCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repo = new();
    private readonly RemoveMenuItemCommandHandler _handler;

    public RemoveMenuItemCommandHandlerTests()
        => _handler = new RemoveMenuItemCommandHandler(_repo.Object);

    private static Restaurant MakeRestaurantWithItem(Guid ownerId, out Guid itemId)
    {
        var r = Restaurant.Create(ownerId, "Bistro", "Good food", "123 Main St");
        var item = r.AddMenuItem("Burger", "Juicy", 8.99m);
        itemId = item.Id;
        return r;
    }

    [Fact]
    public async Task Handle_ValidData_RemovesMenuItem()
    {
        var ownerId = Guid.NewGuid();
        var restaurant = MakeRestaurantWithItem(ownerId, out var itemId);
        _repo.Setup(r => r.GetByIdWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        await _handler.HandleAsync(new RemoveMenuItemCommand(restaurant.Id, itemId, ownerId));

        restaurant.MenuItems.Should().BeEmpty();
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UnknownItemId_ThrowsMenuItemNotFoundException()
    {
        var ownerId = Guid.NewGuid();
        var restaurant = MakeRestaurantWithItem(ownerId, out _);
        _repo.Setup(r => r.GetByIdWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var act = () => _handler.HandleAsync(
            new RemoveMenuItemCommand(restaurant.Id, Guid.NewGuid(), ownerId));

        await act.Should().ThrowAsync<MenuItemNotFoundException>();
    }

    [Fact]
    public async Task Handle_WrongOwner_ThrowsRestaurantOwnershipException()
    {
        var ownerId = Guid.NewGuid();
        var restaurant = MakeRestaurantWithItem(ownerId, out var itemId);
        _repo.Setup(r => r.GetByIdWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var act = () => _handler.HandleAsync(
            new RemoveMenuItemCommand(restaurant.Id, itemId, Guid.NewGuid()));

        await act.Should().ThrowAsync<RestaurantOwnershipException>();
    }
}
