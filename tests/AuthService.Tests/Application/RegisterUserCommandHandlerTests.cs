using AuthService.Application.Commands;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Application.Validators;
using AuthService.Domain.Entities;
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
        _jwt.Setup(j => j.Generate(It.IsAny<User>())).Returns("access-token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _handler = new RegisterUserCommandHandler(_repo.Object, _hasher.Object, _jwt.Object);
    }

    [Fact]
    public async Task Handle_WithNewEmail_CreatesUserSuccessfully()
    {
        _repo.Setup(r => r.ExistsByEmailAsync("new@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(
            new RegisterUserCommand("new@test.com", "pass123", UserRole.Customer));

        result.AccessToken.Should().Be("access-token");
        result.Role.Should().Be("Customer");
        _repo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ThrowsUserAlreadyExistsException()
    {
        _repo.Setup(r => r.ExistsByEmailAsync("dup@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.HandleAsync(
            new RegisterUserCommand("dup@test.com", "pass123", UserRole.Customer));

        await act.Should().ThrowAsync<UserAlreadyExistsException>();
    }

    [Fact]
    public async Task Handle_WithMissingEmail_FailsValidation()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest { Email = "", Password = "validpass", Role = UserRole.Customer };

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Handle_WithShortPassword_FailsValidation()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest { Email = "a@b.com", Password = "abc", Role = UserRole.Customer };

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
