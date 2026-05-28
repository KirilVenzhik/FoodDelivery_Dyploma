using AuthService.Application.Commands;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace AuthService.Tests.Application;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IJwtProvider> _jwtMock = new();
    private readonly RegisterUserCommandHandler _sut;

    public RegisterUserCommandHandlerTests()
    {
        _jwtMock.Setup(x => x.Generate(It.IsAny<User>())).Returns("access-token");
        _jwtMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
        _hasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed-password");
        _repoMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new RegisterUserCommandHandler(_repoMock.Object, _hasherMock.Object, _jwtMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsNew_ReturnsAuthResponse()
    {
        _repoMock.Setup(x => x.ExistsByEmailAsync("new@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.HandleAsync(
            new RegisterUserCommand("new@test.com", "password123", UserRole.Customer));

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.UserId.Should().NotBe(Guid.Empty);
        result.Role.Should().Be("Customer");
    }

    [Fact]
    public async Task HandleAsync_WhenEmailExists_ThrowsUserAlreadyExistsException()
    {
        _repoMock.Setup(x => x.ExistsByEmailAsync("existing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _sut.HandleAsync(
            new RegisterUserCommand("existing@test.com", "password123", UserRole.Customer));

        await act.Should().ThrowAsync<UserAlreadyExistsException>();
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsNew_HashesPassword()
    {
        _repoMock.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.HandleAsync(
            new RegisterUserCommand("new@test.com", "plain-password", UserRole.Customer));

        _hasherMock.Verify(x => x.Hash("plain-password"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsNew_SavesUser()
    {
        _repoMock.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.HandleAsync(
            new RegisterUserCommand("new@test.com", "password123", UserRole.Customer));

        _repoMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailExists_DoesNotSaveUser()
    {
        _repoMock.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        try { await _sut.HandleAsync(new RegisterUserCommand("x@x.com", "pass", UserRole.Customer)); }
        catch (UserAlreadyExistsException) { }

        _repoMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _repoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
