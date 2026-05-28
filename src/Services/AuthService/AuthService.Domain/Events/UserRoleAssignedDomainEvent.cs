using Shared.Primitives;

namespace AuthService.Domain.Events;

public sealed class UserRoleAssignedDomainEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string NewRole { get; init; } = default!;
}
