using AuthService.Application.DTOs;

namespace AuthService.Application.Commands;

public sealed record RefreshTokenCommand(string RefreshToken);

public interface IRefreshTokenCommandHandler
{
    Task<AuthResponse> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default);
}
