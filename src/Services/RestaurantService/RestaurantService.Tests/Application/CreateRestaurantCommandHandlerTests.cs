using FluentAssertions;
using FluentValidation;
using Moq;
using RestaurantService.Application.Commands;
using RestaurantService.Application.Interfaces;
using RestaurantService.Application.Validators;
using RestaurantService.Domain.Entities;

namespace RestaurantService.Tests.Application;

public class CreateRestaurantCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repo = new();
    private readonly IValidator<CreateRestaurantCommand> _validator = new CreateRestaurantCommandValidator();
    private readonly CreateRestaurantCommandHandler _handler;

    public CreateRestaurantCommandHandlerTests()
        => _handler = new CreateRestaurantCommandHandler(_repo.Object, _validator);

    [Fact]
    public async Task Handle_ValidData_ReturnsRestaurantResponse()
    {
        var cmd = new CreateRestaurantCommand(
            Guid.NewGuid(), "Bistro", "Good food", "123 Main St");

        var result = await _handler.HandleAsync(cmd);

        result.Name.Should().Be("Bistro");
        _repo.Verify(r => r.AddAsync(It.IsAny<Restaurant>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyName_ThrowsValidationException()
    {
        var cmd = new CreateRestaurantCommand(
            Guid.NewGuid(), "", "Good food", "123 Main St");

        var act = () => _handler.HandleAsync(cmd);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
