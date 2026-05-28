using Xunit;
using AuthService.API.Controllers;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuthService.Tests.API;

public class AuthControllerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _jwtProviderMock.Setup(x => x.Generate(It.IsAny<User>())).Returns("access-token");
        _jwtProviderMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
        _sut = new AuthController(_userRepositoryMock.Object, _passwordHasherMock.Object, _jwtProviderMock.Object);
    }

    [Fact]
    public async Task Register_WhenEmailNotExists_ReturnsOkWithTokens()
    {
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync("new@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(x => x.Hash("password")).Returns("hashed");
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Register(
            new RegisterRequest { Email = "new@test.com", Password = "password", Role = UserRole.Customer },
            CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<AuthResponse>().Subject;
        response.AccessToken.Should().Be("access-token");
        response.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Register_WhenEmailExists_ReturnsConflict()
    {
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync("existing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.Register(
            new RegisterRequest { Email = "existing@test.com", Password = "password", Role = UserRole.Customer },
            CancellationToken.None);

        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithTokens()
    {
        var user = User.Create("user@test.com", "hashed", UserRole.Customer);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.Verify("password", "hashed")).Returns(true);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Login(
            new LoginRequest { Email = "user@test.com", Password = "password" },
            CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<AuthResponse>();
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        _userRepositoryMock.Setup(x => x.GetByEmailAsync("nobody@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.Login(
            new LoginRequest { Email = "nobody@test.com", Password = "pw" },
            CancellationToken.None);

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var user = User.Create("user@test.com", "hashed", UserRole.Customer);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.Verify("wrong", "hashed")).Returns(false);

        var result = await _sut.Login(
            new LoginRequest { Email = "user@test.com", Password = "wrong" },
            CancellationToken.None);

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        var user = User.Create("user@test.com", "hashed", UserRole.Customer);
        user.SetRefreshToken("valid-token", DateTime.UtcNow.AddDays(1));
        _userRepositoryMock.Setup(x => x.GetByRefreshTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Refresh(
            new RefreshTokenRequest { RefreshToken = "valid-token" },
            CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Refresh_WithExpiredToken_ReturnsUnauthorized()
    {
        var user = User.Create("user@test.com", "hashed", UserRole.Customer);
        user.SetRefreshToken("expired-token", DateTime.UtcNow.AddDays(-1));
        _userRepositoryMock.Setup(x => x.GetByRefreshTokenAsync("expired-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.Refresh(
            new RefreshTokenRequest { RefreshToken = "expired-token" },
            CancellationToken.None);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        _userRepositoryMock.Setup(x => x.GetByRefreshTokenAsync("bad-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.Refresh(
            new RefreshTokenRequest { RefreshToken = "bad-token" },
            CancellationToken.None);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
