using Xunit;
using Auth.Jwt;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AuthService.Tests.Infrastructure;

public class JwtProviderTests
{
    private readonly JwtProvider _sut;

    public JwtProviderTests()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SecretKey = "super-secret-key-that-is-long-enough-32chars",
            ExpirationMinutes = 60
        });
        _sut = new JwtProvider(options);
    }

    [Fact]
    public void Generate_ReturnsNonEmptyString()
    {
        var user = User.Create("user@test.com", "hash", UserRole.Customer);
        var token = _sut.Generate(user);
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Generate_ReturnsValidJwtFormat()
    {
        var user = User.Create("user@test.com", "hash", UserRole.Customer);
        var token = _sut.Generate(user);
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void Generate_DifferentUsersProduceDifferentTokens()
    {
        var user1 = User.Create("user1@test.com", "hash", UserRole.Customer);
        var user2 = User.Create("user2@test.com", "hash", UserRole.Courier);
        _sut.Generate(user1).Should().NotBe(_sut.Generate(user2));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateRefreshToken();
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueTokensOnEachCall()
    {
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();
        token1.Should().NotBe(token2);
    }
}
