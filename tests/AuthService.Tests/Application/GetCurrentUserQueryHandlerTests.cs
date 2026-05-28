using AuthService.Application.Interfaces;
using AuthService.Application.Queries;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace AuthService.Tests.Application;

public class GetCurrentUserQueryHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly GetCurrentUserQueryHandler _sut;

    public GetCurrentUserQueryHandlerTests()
    {
        _sut = new GetCurrentUserQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserExists_ReturnsUserDto()
    {
        var userId = Guid.NewGuid();
        var user = User.Create("user@test.com", "hash", UserRole.Courier);
        _repoMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.HandleAsync(new GetCurrentUserQuery(user.Id));

        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be("user@test.com");
        result.Role.Should().Be("Courier");
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsUserNotFoundException()
    {
        var unknownId = Guid.NewGuid();
        _repoMock.Setup(x => x.GetByIdAsync(unknownId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.HandleAsync(new GetCurrentUserQuery(unknownId));

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WhenUserExists_QueriesCorrectId()
    {
        var user = User.Create("user@test.com", "hash", UserRole.Admin);
        _repoMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _sut.HandleAsync(new GetCurrentUserQuery(user.Id));

        _repoMock.Verify(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserExists_RoleIsStringRepresentation()
    {
        var user = User.Create("admin@test.com", "hash", UserRole.RestaurantOwner);
        _repoMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.HandleAsync(new GetCurrentUserQuery(user.Id));

        result.Role.Should().Be("RestaurantOwner");
    }
}
