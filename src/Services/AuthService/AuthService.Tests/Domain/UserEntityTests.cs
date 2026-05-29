using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Events;
using FluentAssertions;

namespace AuthService.Tests.Domain;

public class UserEntityTests
{
    [Fact]
    public void Create_SetsPropertiesCorrectly()
    {
        var user = User.Create("test@example.com", "hash123", UserRole.Customer);

        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().Be("hash123");
        user.Role.Should().Be(UserRole.Customer);
        user.Id.Should().NotBeEmpty();
        user.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_RaisesUserRegisteredDomainEvent()
    {
        var user = User.Create("test@example.com", "hash", UserRole.Customer);

        user.DomainEvents.Should().ContainSingle(e => e is UserRegisteredDomainEvent);
        var evt = (UserRegisteredDomainEvent)user.DomainEvents.First();
        evt.UserId.Should().Be(user.Id);
        evt.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void SetRefreshToken_StoresTokenAndExpiry()
    {
        var user = User.Create("a@b.com", "h", UserRole.Customer);
        var expiry = DateTime.UtcNow.AddDays(7);

        user.SetRefreshToken("rt123", expiry);

        user.RefreshToken.Should().Be("rt123");
        user.RefreshTokenExpiry.Should().BeCloseTo(expiry, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetRefreshToken_CanBeCalledRepeatedly()
    {
        var user = User.Create("a@b.com", "h", UserRole.Customer);
        user.SetRefreshToken("first", DateTime.UtcNow.AddDays(1));
        user.SetRefreshToken("second", DateTime.UtcNow.AddDays(7));

        user.RefreshToken.Should().Be("second");
    }

    [Fact]
    public void AssignRole_UpdatesRoleAndRaisesEvent()
    {
        var user = User.Create("a@b.com", "h", UserRole.Customer);
        user.ClearDomainEvents();

        user.AssignRole(UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
        user.DomainEvents.Should().ContainSingle(e => e is UserRoleAssignedDomainEvent);
    }

    [Fact]
    public void ChangePassword_UpdatesPasswordHash()
    {
        var user = User.Create("a@b.com", "old", UserRole.Customer);

        user.ChangePassword("newHash");

        user.PasswordHash.Should().Be("newHash");
    }

    [Fact]
    public void UserRole_RestaurantOwner_HasIntValue2()
    {
        ((int)UserRole.RestaurantOwner).Should().Be(2);
    }

    [Fact]
    public void UserRole_Customer_HasIntValue1()
    {
        ((int)UserRole.Customer).Should().Be(1);
    }
}
