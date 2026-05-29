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
    private readonly Mock<IUserRepository> _repo = new();
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _handler = new GetCurrentUserQueryHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsUserDto()
    {
        var user = User.Create("user@test.com", "h", UserRole.Courier);
        _repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.HandleAsync(new GetCurrentUserQuery(user.Id));

        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be("user@test.com");
        result.Role.Should().Be("Courier");
    }

    [Fact]
    public async Task Handle_UnknownId_ThrowsUserNotFoundException()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _handler.HandleAsync(new GetCurrentUserQuery(id));

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task Handle_QueriesCorrectId()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        try { await _handler.HandleAsync(new GetCurrentUserQuery(id)); } catch { }

        _repo.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RoleIsStringRepresentation()
    {
        var user = User.Create("owner@test.com", "h", UserRole.RestaurantOwner);
        _repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.HandleAsync(new GetCurrentUserQuery(user.Id));

        result.Role.Should().Be("RestaurantOwner");
    }
}
