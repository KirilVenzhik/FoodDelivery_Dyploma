using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Exceptions;

namespace AuthService.Application.Commands;

public sealed class RefreshTokenCommandHandler : IRefreshTokenCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(command.RefreshToken, cancellationToken);
        if (user is null || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new InvalidCredentialsException();

        var newRefreshToken = _jwtProvider.GenerateRefreshToken();
        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = _jwtProvider.Generate(user),
            RefreshToken = newRefreshToken,
            UserId = user.Id,
            Role = user.Role.ToString()
        };
    }
}
