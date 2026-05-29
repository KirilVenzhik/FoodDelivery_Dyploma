using FluentAssertions;
using FluentValidation;
using Moq;
using RestaurantService.Application.Commands;
using RestaurantService.Application.Interfaces;
using RestaurantService.Application.Validators;
using RestaurantService.Domain.Entities;

namespace RestaurantService.Tests;

public class CreateRestaurantCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repoMock = new();
    private readonly IValidator<CreateRestaurantCommand> _validator = new CreateRestaurantCommandValidator();

    private CreateRestaurantCommandHandler CreateHandler() =>
        new(_repoMock.Object, _validator);

    [Fact]
    public async Task Handle_WithValidData_CreatesRestaurantAndReturnsResponse()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Restaurant>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateRestaurantCommand(Guid.NewGuid(), "Test", "Desc", "Addr");
        var result = await CreateHandler().HandleAsync(command);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
        result.OwnerId.Should().Be(command.OwnerId);
    }

    [Fact]
    public async Task Handle_WithEmptyName_FailsValidation()
    {
        var command = new CreateRestaurantCommand(Guid.NewGuid(), "", "Desc", "Addr");
        var act = async () => await CreateHandler().HandleAsync(command);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
