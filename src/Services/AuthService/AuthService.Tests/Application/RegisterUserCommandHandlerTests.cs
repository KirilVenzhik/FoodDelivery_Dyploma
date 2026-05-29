using AuthService.Application.Commands;
using AuthService.Application.Interfaces;
using AuthService.Domain.Enums;
using AuthService.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace AuthService.Tests.Application;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtProvider> _jwt = new();
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _jwt.Setup(j => j.Generate(It.IsAny<AuthService.Domain.Entities.User>())).Returns("access-token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _handler = new RegisterUserCommandHandler(_repo.Object, _hasher.Object, _jwt.Object);
    }

    [Fact]
    public async Task Handle_NewEmail_ReturnsAuthResponse()
    {
        _repo.Setup(r => r.ExistsByEmailAsync("new@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(
            new RegisterUserCommand("new@test.com", "pass", UserRole.Customer));

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.Role.Should().Be("Customer");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsUserAlreadyExistsException()
    {
        _repo.Setup(r => r.ExistsByEmailAsync("dup@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.HandleAsync(
            new RegisterUserCommand("dup@test.com", "pass", UserRole.Customer));

        await act.Should().ThrowAsync<UserAlreadyExistsException>();
    }

    [Fact]
    public async Task Handle_HashesPassword()
    {
        _repo.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _handler.HandleAsync(
            new RegisterUserCommand("a@b.com", "plaintext", UserRole.Customer));

        _hasher.Verify(h => h.Hash("plaintext"), Times.Once);
    }

    [Fact]
    public async Task Handle_SavesUser()
    {
        _repo.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _handler.HandleAsync(
            new RegisterUserCommand("a@b.com", "pass", UserRole.Customer));

        _repo.Verify(r => r.AddAsync(It.IsAny<AuthService.Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_DoesNotSave()
    {
        _repo.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        try { await _handler.HandleAsync(new RegisterUserCommand("dup@test.com", "pass", UserRole.Customer)); }
        catch (UserAlreadyExistsException) { }

        _repo.Verify(r => r.AddAsync(It.IsAny<AuthService.Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
