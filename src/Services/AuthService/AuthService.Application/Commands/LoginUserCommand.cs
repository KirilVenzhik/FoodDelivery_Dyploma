using AuthService.Application.DTOs;

namespace AuthService.Application.Commands;

public sealed record LoginUserCommand(string Email, string Password);

public interface ILoginUserCommandHandler
{
    Task<AuthResponse> HandleAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken = default);
}
