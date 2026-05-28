using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Events;
using FluentAssertions;

namespace AuthService.Tests.Domain;

public class UserEntityTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var before = DateTime.UtcNow;
        var user = User.Create("test@example.com", "hash", UserRole.Customer);
        var after = DateTime.UtcNow;

        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().Be("hash");
        user.Role.Should().Be(UserRole.Customer);
        user.CreatedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        user.RefreshToken.Should().BeNull();
        user.RefreshTokenExpiry.Should().BeNull();
    }

    [Fact]
    public void Create_RaisesUserRegisteredDomainEvent()
    {
        var user = User.Create("test@example.com", "hash", UserRole.Customer);

        var evt = user.DomainEvents.OfType<UserRegisteredDomainEvent>().SingleOrDefault();
        evt.Should().NotBeNull();
        evt!.UserId.Should().Be(user.Id);
        evt.Email.Should().Be("test@example.com");
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

    [Fact]
    public void AssignRole_UpdatesRole()
    {
        var user = User.Create("test@example.com", "hash", UserRole.Customer);

        user.AssignRole(UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void AssignRole_RaisesUserRoleAssignedDomainEvent()
    {
        var user = User.Create("test@example.com", "hash", UserRole.Customer);
        user.ClearDomainEvents();

        user.AssignRole(UserRole.Courier);

        var evt = user.DomainEvents.OfType<UserRoleAssignedDomainEvent>().SingleOrDefault();
        evt.Should().NotBeNull();
        evt!.UserId.Should().Be(user.Id);
        evt.NewRole.Should().Be("Courier");
    }

    [Fact]
    public void ChangePassword_UpdatesPasswordHash()
    {
        var user = User.Create("test@example.com", "old-hash", UserRole.Customer);

        user.ChangePassword("new-hash");

        user.PasswordHash.Should().Be("new-hash");
    }

    [Fact]
    public void UserRole_HasCorrectValues()
    {
        ((int)UserRole.Customer).Should().Be(1);
        ((int)UserRole.RestaurantOwner).Should().Be(2);
        ((int)UserRole.Courier).Should().Be(3);
        ((int)UserRole.Admin).Should().Be(4);
    }
}
