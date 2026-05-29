using AuthService.Application.Interfaces;
using AuthService.Application.Queries;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
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
    public async Task Handle_WithExistingUserId_ReturnsUserDto()
    {
        var user = User.Create("user@test.com", "h", UserRole.Courier);
        _repo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.HandleAsync(new GetCurrentUserQuery(user.Id));

        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be("user@test.com");
        result.Role.Should().Be("Courier");
    }
}
