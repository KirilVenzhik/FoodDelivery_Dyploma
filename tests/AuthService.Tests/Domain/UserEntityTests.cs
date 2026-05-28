using Xunit;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using FluentAssertions;

namespace AuthService.Tests.Domain;

public class UserEntityTests
{
    [Fact]
    public void Create_SetsIdAsNewGuid()
    {
        var user = User.Create("test@example.com", "hash", UserRole.Customer);
        user.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_SetsEmail()
    {
        var user = User.Create("test@example.com", "hash", UserRole.Customer);
        user.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_SetsPasswordHash()
    {
        var user = User.Create("test@example.com", "hashed_pw", UserRole.Customer);
        user.PasswordHash.Should().Be("hashed_pw");
    }

    [Fact]
    public void Create_SetsRole()
    {
        var user = User.Create("test@example.com", "hash", UserRole.RestaurantManager);
        user.Role.Should().Be(UserRole.RestaurantManager);
    }

    [Fact]
    public void Create_SetsCreatedAtUtcToNow()
    {
        var before = DateTime.UtcNow;
        var user = User.Create("test@example.com", "hash", UserRole.Customer);
        var after = DateTime.UtcNow;
        user.CreatedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_RefreshTokenIsNull()
    {
        var user = User.Create("test@example.com", "hash", UserRole.Customer);
        user.RefreshToken.Should().BeNull();
        user.RefreshTokenExpiry.Should().BeNull();
    }

    [Fact]
    public void SetRefreshToken_UpdatesTokenAndExpiry()
    {
        var user = User.Create("test@example.com", "hash", UserRole.Customer);
        var expiry = DateTime.UtcNow.AddDays(7);

        user.SetRefreshToken("my-token", expiry);

        user.RefreshToken.Should().Be("my-token");
        user.RefreshTokenExpiry.Should().Be(expiry);
    }

    [Fact]
    public void SetRefreshToken_CanBeCalledMultipleTimes()
    {
        var user = User.Create("test@example.com", "hash", UserRole.Customer);

        user.SetRefreshToken("token-1", DateTime.UtcNow.AddDays(7));
        user.SetRefreshToken("token-2", DateTime.UtcNow.AddDays(14));

        user.RefreshToken.Should().Be("token-2");
    }
}
