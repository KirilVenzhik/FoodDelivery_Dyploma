using AuthService.Application.DTOs;
using AuthService.Domain.Enums;

namespace AuthService.Application.Commands;

public sealed record RegisterUserCommand(string Email, string Password, UserRole Role);

public interface IRegisterUserCommandHandler
{
    Task<AuthResponse> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default);
}
