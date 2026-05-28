using Shared.Primitives;

namespace AuthService.Domain.Events;

public sealed class UserRegisteredDomainEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = default!;
}
