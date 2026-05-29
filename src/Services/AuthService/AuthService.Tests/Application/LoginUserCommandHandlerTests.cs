using AuthService.Application.Commands;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace AuthService.Tests.Application;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtProvider> _jwt = new();
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _jwt.Setup(j => j.Generate(It.IsAny<User>())).Returns("access-token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _handler = new LoginUserCommandHandler(_repo.Object, _hasher.Object, _jwt.Object);
    }

    private static User MakeUser()
        => User.Create("user@test.com", "hashedPass", UserRole.Customer);

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponse()
    {
        var user = MakeUser();
        _repo.Setup(r => r.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("pass", "hashedPass")).Returns(true);

        var result = await _handler.HandleAsync(new LoginUserCommand("user@test.com", "pass"));

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_UnknownEmail_ThrowsInvalidCredentialsException()
    {
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _handler.HandleAsync(new LoginUserCommand("nobody@test.com", "pass"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsInvalidCredentialsException()
    {
        var user = MakeUser();
        _repo.Setup(r => r.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("wrong", "hashedPass")).Returns(false);

        var act = () => _handler.HandleAsync(new LoginUserCommand("user@test.com", "wrong"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Handle_ValidCredentials_SetsRefreshTokenOnUser()
    {
        var user = MakeUser();
        _repo.Setup(r => r.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("pass", "hashedPass")).Returns(true);

        await _handler.HandleAsync(new LoginUserCommand("user@test.com", "pass"));

        user.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_InvalidCredentials_DoesNotSave()
    {
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        try { await _handler.HandleAsync(new LoginUserCommand("x@x.com", "pass")); }
        catch (InvalidCredentialsException) { }

        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
