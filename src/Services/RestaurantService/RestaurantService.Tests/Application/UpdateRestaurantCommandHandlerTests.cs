using FluentAssertions;
using Moq;
using RestaurantService.Application.Commands;
using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Entities;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Tests.Application;

public class UpdateRestaurantCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repo = new();
    private readonly UpdateRestaurantCommandHandler _handler;

    public UpdateRestaurantCommandHandlerTests()
        => _handler = new UpdateRestaurantCommandHandler(_repo.Object);

    private static Restaurant MakeRestaurant(Guid ownerId)
        => Restaurant.Create(ownerId, "Old Name", "Old desc", "Old addr");

    [Fact]
    public async Task Handle_ValidOwner_UpdatesRestaurant()
    {
        var ownerId = Guid.NewGuid();
        var restaurant = MakeRestaurant(ownerId);
        _repo.Setup(r => r.GetByIdAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var result = await _handler.HandleAsync(
            new UpdateRestaurantCommand(restaurant.Id, ownerId, "New Name", "New desc", "New addr"));

        result.Name.Should().Be("New Name");
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WrongOwner_ThrowsRestaurantOwnershipException()
    {
        var restaurant = MakeRestaurant(Guid.NewGuid());
        _repo.Setup(r => r.GetByIdAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var act = () => _handler.HandleAsync(
            new UpdateRestaurantCommand(restaurant.Id, Guid.NewGuid(), "x", "y", "z"));

        await act.Should().ThrowAsync<RestaurantOwnershipException>();
    }

    [Fact]
    public async Task Handle_UnknownId_ThrowsRestaurantNotFoundException()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Restaurant?)null);

        var act = () => _handler.HandleAsync(
            new UpdateRestaurantCommand(id, Guid.NewGuid(), "x", "y", "z"));

        await act.Should().ThrowAsync<RestaurantNotFoundException>();
    }
}
