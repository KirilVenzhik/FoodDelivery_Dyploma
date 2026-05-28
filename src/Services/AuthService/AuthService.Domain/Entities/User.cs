using AuthService.Domain.Enums;
using AuthService.Domain.Events;
using Shared.Primitives;

namespace AuthService.Domain.Entities;

public class User : Entity
{
    private User() { }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = default!;

    public string PasswordHash { get; private set; } = default!;

    public UserRole Role { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public string? RefreshToken { get; private set; }

    public DateTime? RefreshTokenExpiry { get; private set; }

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
    }

    public void AssignRole(UserRole role)
    {
        Role = role;
        RaiseDomainEvent(new UserRoleAssignedDomainEvent
        {
            UserId = Id,
            NewRole = role.ToString()
        });
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public static User Create(string email, string passwordHash, UserRole role)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            Role = role,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.RaiseDomainEvent(new UserRegisteredDomainEvent
        {
            UserId = user.Id,
            Email = user.Email
        });

        return user;
    }
}
