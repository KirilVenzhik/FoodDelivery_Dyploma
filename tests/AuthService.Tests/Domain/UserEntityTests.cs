using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Events;
using FluentAssertions;

namespace AuthService.Tests.Domain;

public class UserEntityTests
{
    [Fact]
    public void Create_WithValidData_ReturnsUser()
    {
        var user = User.Create("test@example.com", "hash123", UserRole.Customer);

        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithValidData_SetsCorrectEmail()
    {
        var user = User.Create("test@example.com", "hash123", UserRole.Customer);

        user.Email.Should().Be("test@example.com");
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
    public void AssignRole_ChangesRole()
    {
        var user = User.Create("a@b.com", "h", UserRole.Customer);

        user.AssignRole(UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void AssignRole_RaisesUserRoleAssignedDomainEvent()
    {
        var user = User.Create("a@b.com", "h", UserRole.Customer);
        user.ClearDomainEvents();

        user.AssignRole(UserRole.Admin);

        user.DomainEvents.Should().ContainSingle(e => e is UserRoleAssignedDomainEvent);
    }

    [Fact]
    public void ChangePassword_UpdatesPasswordHash()
    {
        var user = User.Create("a@b.com", "old", UserRole.Customer);

        user.ChangePassword("newHash");

        user.PasswordHash.Should().Be("newHash");
    }
}
