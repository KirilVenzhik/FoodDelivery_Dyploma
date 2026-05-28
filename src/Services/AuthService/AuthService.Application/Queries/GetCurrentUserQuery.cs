using AuthService.Application.DTOs;

namespace AuthService.Application.Queries;

public sealed record GetCurrentUserQuery(Guid UserId);

public interface IGetCurrentUserQueryHandler
{
    Task<UserDto> HandleAsync(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken = default);
}
