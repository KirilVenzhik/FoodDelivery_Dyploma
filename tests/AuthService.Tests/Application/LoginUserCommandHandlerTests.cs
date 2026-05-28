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
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IJwtProvider> _jwtMock = new();
    private readonly LoginUserCommandHandler _sut;

    public LoginUserCommandHandlerTests()
    {
        _jwtMock.Setup(x => x.Generate(It.IsAny<User>())).Returns("access-token");
        _jwtMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
        _repoMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new LoginUserCommandHandler(_repoMock.Object, _hasherMock.Object, _jwtMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        var user = User.Create("user@test.com", "hashed", UserRole.Customer);
        _repoMock.Setup(x => x.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(x => x.Verify("password", "hashed")).Returns(true);

        var result = await _sut.HandleAsync(new LoginUserCommand("user@test.com", "password"));

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.UserId.Should().Be(user.Id);
        result.Role.Should().Be("Customer");
    }

    [Fact]
    public async Task HandleAsync_WithUnknownEmail_ThrowsInvalidCredentialsException()
    {
        _repoMock.Setup(x => x.GetByEmailAsync("nobody@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.HandleAsync(new LoginUserCommand("nobody@test.com", "pass"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task HandleAsync_WithWrongPassword_ThrowsInvalidCredentialsException()
    {
        var user = User.Create("user@test.com", "hashed", UserRole.Customer);
        _repoMock.Setup(x => x.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(x => x.Verify("wrong", "hashed")).Returns(false);

        var act = () => _sut.HandleAsync(new LoginUserCommand("user@test.com", "wrong"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_SetsRefreshTokenOnUser()
    {
        var user = User.Create("user@test.com", "hashed", UserRole.Customer);
        _repoMock.Setup(x => x.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(x => x.Verify("password", "hashed")).Returns(true);

        await _sut.HandleAsync(new LoginUserCommand("user@test.com", "password"));

        user.RefreshToken.Should().Be("refresh-token");
        user.RefreshTokenExpiry.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task HandleAsync_WithUnknownEmail_DoesNotSaveChanges()
    {
        _repoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        try { await _sut.HandleAsync(new LoginUserCommand("x@x.com", "pass")); }
        catch (InvalidCredentialsException) { }

        _repoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
