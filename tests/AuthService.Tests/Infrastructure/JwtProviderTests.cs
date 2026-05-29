using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth.Jwt;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AuthService.Tests.Infrastructure;

public class JwtProviderTests
{
    private readonly JwtProvider _provider;

    public JwtProviderTests()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SecretKey = "super-secret-key-that-is-long-enough-for-hmac",
            ExpirationMinutes = 60
        });
        _provider = new JwtProvider(options);
    }

    private static User MakeUser()
        => User.Create("test@example.com", "hash", UserRole.Customer);

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        var user = MakeUser();

        var token = _provider.Generate(user);

        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_ContainsUserIdClaim()
    {
        var user = MakeUser();

        var token = _provider.Generate(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
    }

    [Fact]
    public void GenerateToken_ContainsRoleClaim()
    {
        var user = MakeUser();

        var token = _provider.Generate(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.Role && c.Value == "Customer");
    }
}
