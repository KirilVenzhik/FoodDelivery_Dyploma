using FluentAssertions;
using FluentValidation;
using Moq;
using RestaurantService.Application.Commands;
using RestaurantService.Application.Interfaces;
using RestaurantService.Application.Validators;
using RestaurantService.Domain.Entities;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Tests;

public class AddMenuItemCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repoMock = new();
    private readonly IValidator<AddMenuItemCommand> _validator = new AddMenuItemCommandValidator();

    private AddMenuItemCommandHandler CreateHandler() =>
        new(_repoMock.Object, _validator);

    [Fact]
    public async Task Handle_WithValidData_AddsItemAndReturnsResponse()
    {
        var ownerId = Guid.NewGuid();
        var restaurant = Restaurant.Create(ownerId, "Test", "Desc", "Addr");
        _repoMock.Setup(r => r.GetByIdWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new AddMenuItemCommand(restaurant.Id, ownerId, "Burger", "Tasty", 9.99m);
        var result = await CreateHandler().HandleAsync(command);

        result.Should().NotBeNull();
        result.Name.Should().Be("Burger");
    }

    [Fact]
    public async Task Handle_WithWrongOwner_ThrowsRestaurantOwnershipException()
    {
        var restaurant = Restaurant.Create(Guid.NewGuid(), "Test", "Desc", "Addr");
        _repoMock.Setup(r => r.GetByIdWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var command = new AddMenuItemCommand(restaurant.Id, Guid.NewGuid(), "Burger", "Tasty", 9.99m);
        var act = async () => await CreateHandler().HandleAsync(command);
        await act.Should().ThrowAsync<RestaurantOwnershipException>();
    }

    [Fact]
    public async Task Handle_WithNegativePrice_FailsValidation()
    {
        var command = new AddMenuItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Burger", "Tasty", -1m);
        var act = async () => await CreateHandler().HandleAsync(command);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
