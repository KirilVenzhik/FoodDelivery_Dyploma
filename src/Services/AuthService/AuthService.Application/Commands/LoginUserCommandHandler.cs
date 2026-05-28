using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Exceptions;

namespace AuthService.Application.Commands;

public sealed class LoginUserCommandHandler : ILoginUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse> HandleAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null)
            throw new InvalidCredentialsException();

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        var refreshToken = _jwtProvider.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = _jwtProvider.Generate(user),
            RefreshToken = refreshToken,
            UserId = user.Id,
            Role = user.Role.ToString()
        };
    }
}
