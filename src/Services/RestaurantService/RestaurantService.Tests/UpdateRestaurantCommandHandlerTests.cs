using FluentAssertions;
using Moq;
using RestaurantService.Application.Commands;
using RestaurantService.Application.Interfaces;
using RestaurantService.Domain.Entities;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Tests;

public class UpdateRestaurantCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repoMock = new();

    private UpdateRestaurantCommandHandler CreateHandler() =>
        new(_repoMock.Object);

    [Fact]
    public async Task Handle_WithValidOwner_UpdatesRestaurant()
    {
        var ownerId = Guid.NewGuid();
        var restaurant = Restaurant.Create(ownerId, "Old", "OldDesc", "OldAddr");
        _repoMock.Setup(r => r.GetByIdAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateRestaurantCommand(restaurant.Id, ownerId, "New", "NewDesc", "NewAddr");
        var result = await CreateHandler().HandleAsync(command);

        result.Name.Should().Be("New");
    }

    [Fact]
    public async Task Handle_WithWrongOwner_ThrowsRestaurantOwnershipException()
    {
        var restaurant = Restaurant.Create(Guid.NewGuid(), "Old", "OldDesc", "OldAddr");
        _repoMock.Setup(r => r.GetByIdAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var command = new UpdateRestaurantCommand(restaurant.Id, Guid.NewGuid(), "N", "D", "A");
        var act = async () => await CreateHandler().HandleAsync(command);
        await act.Should().ThrowAsync<RestaurantOwnershipException>();
    }

    [Fact]
    public async Task Handle_WithUnknownId_ThrowsRestaurantNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Restaurant?)null);

        var command = new UpdateRestaurantCommand(Guid.NewGuid(), Guid.NewGuid(), "N", "D", "A");
        var act = async () => await CreateHandler().HandleAsync(command);
        await act.Should().ThrowAsync<RestaurantNotFoundException>();
    }
}
