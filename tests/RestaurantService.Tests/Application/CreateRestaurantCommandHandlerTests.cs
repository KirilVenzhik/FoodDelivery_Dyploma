using FluentAssertions;
using FluentValidation;
using Moq;
using RestaurantService.Application.Commands;
using RestaurantService.Application.Interfaces;
using RestaurantService.Application.Validators;
using RestaurantService.Domain.Entities;
using RestaurantService.Domain.Exceptions;

namespace RestaurantService.Tests.Application;

public class CreateRestaurantCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repo = new();
    private readonly IValidator<CreateRestaurantCommand> _validator = new CreateRestaurantCommandValidator();
    private readonly CreateRestaurantCommandHandler _createHandler;

    public CreateRestaurantCommandHandlerTests()
        => _createHandler = new CreateRestaurantCommandHandler(_repo.Object, _validator);

    [Fact]
    public async Task Handle_WithValidData_CreatesRestaurantSuccessfully()
    {
        var cmd = new CreateRestaurantCommand(Guid.NewGuid(), "Bistro", "Good food", "123 Main St");

        var result = await _createHandler.HandleAsync(cmd);

        result.Name.Should().Be("Bistro");
        _repo.Verify(r => r.AddAsync(It.IsAny<Restaurant>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithWrongOwner_ThrowsRestaurantOwnershipException()
    {
        var ownerId = Guid.NewGuid();
        var restaurant = Restaurant.Create(ownerId, "Old Name", "Old desc", "Old addr");
        _repo.Setup(r => r.GetByIdAsync(restaurant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var updateHandler = new UpdateRestaurantCommandHandler(_repo.Object);
        var act = () => updateHandler.HandleAsync(
            new UpdateRestaurantCommand(restaurant.Id, Guid.NewGuid(), "New Name", "New desc", "New addr"));

        await act.Should().ThrowAsync<RestaurantOwnershipException>();
    }
}
