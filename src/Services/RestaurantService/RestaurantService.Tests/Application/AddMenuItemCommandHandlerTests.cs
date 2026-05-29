using FluentAssertions;
using FluentValidation;
using Moq;
using RestaurantService.Application.Commands;
using RestaurantService.Application.Interfaces;
using RestaurantService.Application.Validators;
using RestaurantService.Domain.Entities;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Tests.Application;

public class AddMenuItemCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repo = new();
    private readonly IValidator<AddMenuItemCommand> _validator = new AddMenuItemCommandValidator();
    private readonly AddMenuItemCommandHandler _handler;

    public AddMenuItemCommandHandlerTests()
        => _handler = new AddMenuItemCommandHandler(_repo.Object, _validator);

    private static Restaurant MakeRestaurant(Guid ownerId)
        => Restaurant.Create(ownerId, "Bistro", "Good food", "123 Main St");

    [Fact]
    public async Task Handle_ValidData_ReturnsMenuItemResponse()
    {
        var ownerId = Guid.NewGuid();
        var restaurant = MakeRestaurant(ownerId);
        _repo.Setup(r => r.GetByIdWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var result = await _handler.HandleAsync(
            new AddMenuItemCommand(restaurant.Id, ownerId, "Burger", "Juicy", 8.99m));

        result.Name.Should().Be("Burger");
        result.Price.Should().Be(8.99m);
    }

    [Fact]
    public async Task Handle_WrongOwner_ThrowsRestaurantOwnershipException()
    {
        var restaurant = MakeRestaurant(Guid.NewGuid());
        _repo.Setup(r => r.GetByIdWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var act = () => _handler.HandleAsync(
            new AddMenuItemCommand(restaurant.Id, Guid.NewGuid(), "Burger", "Juicy", 8.99m));

        await act.Should().ThrowAsync<RestaurantOwnershipException>();
    }

    [Fact]
    public async Task Handle_NegativePrice_ThrowsValidationException()
    {
        var ownerId = Guid.NewGuid();
        var restaurant = MakeRestaurant(ownerId);
        _repo.Setup(r => r.GetByIdWithItemsAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var act = () => _handler.HandleAsync(
            new AddMenuItemCommand(restaurant.Id, ownerId, "Burger", "desc", -1m));

        await act.Should().ThrowAsync<ValidationException>();
    }
}
