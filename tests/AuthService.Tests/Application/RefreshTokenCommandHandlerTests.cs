using AuthService.Application.Commands;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace AuthService.Tests.Application;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<IJwtProvider> _jwtMock = new();
    private readonly RefreshTokenCommandHandler _sut;

    public RefreshTokenCommandHandlerTests()
    {
        _jwtMock.Setup(x => x.Generate(It.IsAny<User>())).Returns("new-access-token");
        _jwtMock.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh-token");
        _repoMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new RefreshTokenCommandHandler(_repoMock.Object, _jwtMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidToken_ReturnsNewAuthResponse()
    {
        var user = User.Create("user@test.com", "hash", UserRole.Customer);
        user.SetRefreshToken("valid-token", DateTime.UtcNow.AddDays(1));
        _repoMock.Setup(x => x.GetByRefreshTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.HandleAsync(new RefreshTokenCommand("valid-token"));

        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        result.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidToken_ThrowsInvalidCredentialsException()
    {
        _repoMock.Setup(x => x.GetByRefreshTokenAsync("bad-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.HandleAsync(new RefreshTokenCommand("bad-token"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task HandleAsync_WithExpiredToken_ThrowsInvalidCredentialsException()
    {
        var user = User.Create("user@test.com", "hash", UserRole.Customer);
        user.SetRefreshToken("expired-token", DateTime.UtcNow.AddDays(-1));
        _repoMock.Setup(x => x.GetByRefreshTokenAsync("expired-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = () => _sut.HandleAsync(new RefreshTokenCommand("expired-token"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task HandleAsync_WithValidToken_RotatesRefreshToken()
    {
        var user = User.Create("user@test.com", "hash", UserRole.Customer);
        user.SetRefreshToken("old-token", DateTime.UtcNow.AddDays(1));
        _repoMock.Setup(x => x.GetByRefreshTokenAsync("old-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _sut.HandleAsync(new RefreshTokenCommand("old-token"));

        user.RefreshToken.Should().Be("new-refresh-token");
        _repoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
