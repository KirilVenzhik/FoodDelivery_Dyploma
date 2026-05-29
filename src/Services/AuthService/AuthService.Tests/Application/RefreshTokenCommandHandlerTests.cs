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
    private readonly Mock<IUserRepository> _repo = new();
    private readonly Mock<IJwtProvider> _jwt = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _jwt.Setup(j => j.Generate(It.IsAny<User>())).Returns("new-access-token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh-token");
        _handler = new RefreshTokenCommandHandler(_repo.Object, _jwt.Object);
    }

    private static User MakeUserWithToken(string token, DateTime expiry)
    {
        var user = User.Create("a@b.com", "h", UserRole.Customer);
        user.SetRefreshToken(token, expiry);
        return user;
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsNewAuthResponse()
    {
        var user = MakeUserWithToken("valid-rt", DateTime.UtcNow.AddDays(1));
        _repo.Setup(r => r.GetByRefreshTokenAsync("valid-rt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.HandleAsync(new RefreshTokenCommand("valid-rt"));

        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsInvalidCredentialsException()
    {
        _repo.Setup(r => r.GetByRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _handler.HandleAsync(new RefreshTokenCommand("bad-token"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsInvalidCredentialsException()
    {
        var user = MakeUserWithToken("expired-rt", DateTime.UtcNow.AddDays(-1));
        _repo.Setup(r => r.GetByRefreshTokenAsync("expired-rt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = () => _handler.HandleAsync(new RefreshTokenCommand("expired-rt"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Handle_ValidToken_RotatesRefreshTokenAndPersists()
    {
        var user = MakeUserWithToken("old-rt", DateTime.UtcNow.AddDays(1));
        _repo.Setup(r => r.GetByRefreshTokenAsync("old-rt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _handler.HandleAsync(new RefreshTokenCommand("old-rt"));

        user.RefreshToken.Should().Be("new-refresh-token");
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
